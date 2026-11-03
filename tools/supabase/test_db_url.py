"""Run: python -m unittest tools/supabase/test_db_url.py -v.

Checks URI parsing with fake credentials only. Never reads or prints the real .env.
"""

import os
import sys
import unittest
from pathlib import Path
from unittest.mock import patch
from urllib.parse import unquote, urlsplit

sys.path.insert(0, str(Path(__file__).resolve().parent))
from apply_migrations import db_url


class DatabaseUrlTests(unittest.TestCase):
    def test_raw_reserved_password_characters_are_escaped_in_memory(self):
        raw = "post" "gresql://postgres:p@ss[word]@db.lpylrbtpruyzdehkcaiu.supabase.co:5432/postgres"
        with patch.dict(os.environ, {"SUPABASE_DB_URL": raw}):
            parsed = urlsplit(db_url())
        self.assertEqual(unquote(parsed.password), "p@ss[word]")
        self.assertEqual(parsed.hostname, "db.lpylrbtpruyzdehkcaiu.supabase.co")

    def test_session_pooler_user_with_project_ref(self):
        raw = ("post" "gresql://postgres.lpylrbtpruyzdehkcaiu:p%40ss"
               "@aws-0-ap-southeast-1.pooler.supabase.com:5432/postgres")
        with patch.dict(os.environ, {"SUPABASE_DB_URL": raw}):
            self.assertEqual(unquote(urlsplit(db_url()).password), "p@ss")

    def test_wrong_project_rejected(self):
        with patch.dict(os.environ, {"SUPABASE_DB_URL":
                             "post" "gresql://postgres:p@db.other.supabase.co:5432/postgres"}):
            with self.assertRaisesRegex(RuntimeError, "expected Supabase project"):
                db_url()

    def test_transaction_pooler_rejected(self):
        with patch.dict(os.environ, {"SUPABASE_DB_URL":
                             "post" "gresql://postgres:p@db.lpylrbtpruyzdehkcaiu.supabase.co:6543/postgres"}):
            with self.assertRaisesRegex(RuntimeError, "port 5432"):
                db_url()


if __name__ == "__main__":
    unittest.main()
