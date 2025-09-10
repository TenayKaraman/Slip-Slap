# -*- coding: utf-8 -*-
#!/usr/bin/env python3
"""
ai_bridge.py
Unity <-> AI k�pr�s�.

�zellikler:
- AI_Project_Context/ai_prompt.txt dosyas�n� dinler.
- Gerekirse context dosyalar�n� (folder_structure.md, scripts_list.md, scene_hierarchy.md) prompt ile birlikte g�nderir.
- OpenAI Python SDK (from openai import OpenAI) kullan�r.
- API key'i �nce ortam de�i�keninden al�r; yoksa secret_key.txt'den okur.
- D�nen cevab� AI_Project_Context/ai_response.json olarak yazar.
- Basit retry ve logging i�erir.

G�venlik notu: secret_key.txt kullan�yorsan dosyay� .gitignore'a ekle ve asla public repo'ya pushlama.
"""

import os
import time
import json
import sys
from pathlib import Path
from typing import Optional

# openai paketinin modern SDK's� kullan�l�yor
try:
    from openai import OpenAI
except Exception as e:
    print("openai paketini bulamad�m. 'pip install openai' ile kur. Hata:", e)
    sys.exit(1)

# ---------- AYARLAR ----------
PROJECT_ROOT = Path(__file__).resolve().parent
CONTEXT_DIR = PROJECT_ROOT / "AI_Project_Context"
PROMPT_FILE = CONTEXT_DIR / "ai_prompt.txt"
RESPONSE_FILE = CONTEXT_DIR / "ai_response.json"
# Context olarak g�nderilecek (varsa) dosyalar
CONTEXT_FILES = [
    CONTEXT_DIR / "folder_structure.md",
    CONTEXT_DIR / "scripts_list.md",
    CONTEXT_DIR / "scene_hierarchy.md",
]
# Model tercihi (kullan�labilir modele g�re g�ncelle)
MODEL_NAME = "gpt-5"  # hesab�nda eri�im varsa; yoksa gpt-4o, gpt-4 veya gpt-3.5-turbo'ya d��
# Timeout ve bekleme
POLL_INTERVAL_SECONDS = 1.5
MAX_RETRIES = 3

# ---------- YARDIMCI FONKS�YONLAR ----------
def load_api_key() -> Optional[str]:
    """�nce ortam de�i�kenine bak, yoksa secret_key.txt oku."""
    key = os.environ.get("OPENAI_API_KEY")
    if key:
        return key.strip()
    secret_path = PROJECT_ROOT / "secret_key.txt"
    if secret_path.exists():
        try:
            return secret_path.read_text(encoding="utf-8").strip()
        except Exception:
            return None
    return None

def read_prompt() -> Optional[str]:
    """Prompt dosyas�n� oku (varsa)."""
    if not PROMPT_FILE.exists():
        return None
    try:
        text = PROMPT_FILE.read_text(encoding="utf-8").strip()
        return text if text else None
    except Exception as e:
        print("Prompt okunurken hata:", e)
        return None

def build_system_message() -> str:
    """Assistant i�in sistem mesaj� (AI'ye nas�l davranmas� gerekti�ini s�yle)."""
    return (
        "Sen bir Unity AI asistan�s�n. Kullan�c�n�n verdi�i talimatlar� sadece ve yaln�zca "
        "ai_response.json format�nda 'commands' listesi d�necek �ekilde cevapla. "
        "Komutlar JSON format�nda olmal� ve �rnek eylemler: create, add_component, delete, set_property, create_prefab. "
        "Her komut �u alanlar� i�ermelidir: action, target, param (gerekiyorsa), value (gerekiyorsa). "
        "Ek a��klama veya do�al dil d�nd�rme. Sadece JSON d�nd�r."
    )

def build_user_messages(prompt: str) -> list:
    """Prompt'a context dosyalar�ndan i�erik ekleyerek mesaj listesi haz�rla."""
    messages = []
    # Kullan�c� promptunu ekle
    messages.append({"role": "user", "content": prompt})

    # Context dosyalar�n� ekle (uzun olabilir; dikkat)
    for p in CONTEXT_FILES:
        if p.exists():
            try:
                content = p.read_text(encoding="utf-8")
                # k�sa bir ba�l�kla ekle
                messages.append({
                    "role": "user",
                    "content": f"--- CONTEXT FILE: {p.name} ---\n{content}\n--- END OF {p.name} ---"
                })
            except Exception:
                pass
    return messages

# ---------- ANA K�PR� FONKS�YONU ----------
def main_loop():
    api_key = load_api_key()
    if not api_key:
        print("OpenAI API key bulunamad�. L�tfen OPENAI_API_KEY ortam de�i�keni ayarla veya project root'a secret_key.txt ekle.")
        return

    # OpenAI client
    client = OpenAI(api_key=api_key)

    print("AI Bridge �al���yor. Prompt bekleniyor... (Ctrl+C ile ��k)")

    while True:
        try:
            prompt = read_prompt()
            if not prompt:
                time.sleep(POLL_INTERVAL_SECONDS)
                continue

            print("Prompt al�nd�:", repr(prompt[:200]) + ("..." if len(prompt) > 200 else ""))

            system_msg = build_system_message()
            user_msgs = build_user_messages(prompt)

            # Hata tolerans� ve retry
            for attempt in range(1, MAX_RETRIES + 1):
                try:
                    response = client.chat.completions.create(
                        model=MODEL_NAME,
                        messages=[{"role": "system", "content": system_msg}] + user_msgs,
                        # opsiyonel: max_tokens, temperature vb.
                        max_tokens=1500,
                        temperature=0.0,
                    )
                    # modern SDK -> response.choices[0].message.content gibi eri�im
                    content = response.choices[0].message.content
                    # Basit do�rulama: JSON mu diye dene
                    try:
                        parsed = json.loads(content)
                    except Exception:
                        # E�er AI d�z metin d�nd�rd�yse, "i�inden JSON'u ay�kla" denenebilir.
                        # Basit heuristic: ilk '{' ile son '}' aras�n� al�p parse etmeye �al��
                        start = content.find("{")
                        end = content.rfind("}")
                        if start != -1 and end != -1 and end > start:
                            snippet = content[start:end+1]
                            parsed = json.loads(snippet)
                        else:
                            raise ValueError("AI yan�t�ndan JSON ��kart�lam�yor.")

                    # Kaydet ve prompt'u temizle
                    RESPONSE_FILE.write_text(json.dumps(parsed, indent=2, ensure_ascii=False), encoding="utf-8")
                    print(f"Yan�t kaydedildi: {RESPONSE_FILE}")
                    # prompt'u temizleyerek tekrar �al��may� bekleriz
                    PROMPT_FILE.write_text("", encoding="utf-8")
                    break  # retry d�ng�s�nden ��k
                except Exception as e:
                    print(f"�stek hatas� (deneme {attempt}/{MAX_RETRIES}):", e)
                    if attempt == MAX_RETRIES:
                        print("Maksimum deneme say�s�na ula��ld�. Hata detay:", e)
                    else:
                        time.sleep(1.0 * attempt)
            # D�ng�de bekle
            time.sleep(POLL_INTERVAL_SECONDS)
        except KeyboardInterrupt:
            print("AI Bridge durduruldu (Ctrl+C).")
            break
        except Exception as e:
            print("Beklenmedik hata:", e)
            time.sleep(2.0)

if __name__ == "__main__":
    # Klas�rleri olu�tur
    CONTEXT_DIR.mkdir(parents=True, exist_ok=True)
    # Bo� response dosyas� varsa overwrite etmeyelim, ama yoksa olu�tur
    if not RESPONSE_FILE.exists():
        RESPONSE_FILE.write_text("{}", encoding="utf-8")
    main_loop()
