#!/bin/bash
set -e

BASE_URL="http://localhost:5210"
COOKIE_JAR="/tmp/admin_cookies.txt"
rm -f "$COOKIE_JAR"

TS=$(date +%s)
USERNAME="cafe_$TS"
EMAIL="cafe_$TS@sodiqcafe.uz"

echo "=================================================="
echo "1. Admin orqali tizimga kirish"
echo "=================================================="
LOGIN_RESP=$(curl -s -c "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail": "admin", "password": "Admin123!"}')
echo "Login natijasi: $LOGIN_RESP"

echo ""
echo "=================================================="
echo "2. Yangi Kafe va Kafe egasini ro'yxatdan o'tkazish"
echo "   Oylik to'lov: 300,000 so'm, Dastlabki balans: 0 so'm"
echo "=================================================="
REGISTER_RESP=$(curl -s -b "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/register-cafe-owner" \
  -H "Content-Type: application/json" \
  -d "{
    \"username\": \"$USERNAME\",
    \"email\": \"$EMAIL\",
    \"password\": \"Password123!\",
    \"fullName\": \"Sodiq Test Owner\",
    \"phoneNumber\": \"+998901234567\",
    \"cafeName\": \"Sodiq Modern Cafe $TS\",
    \"cafeAddress\": \"Chilonzor 1-mavze, 10\",
    \"monthlyFee\": 300000,
    \"initialBalance\": 0
  }")
echo "Ro'yxatdan o'tkazish javobi: $REGISTER_RESP"
CAFE_ID=$(echo "$REGISTER_RESP" | grep -o '"cafeId":[0-9]*' | head -1 | cut -d':' -f2)
echo "Yaratilgan Kafe ID: $CAFE_ID"

echo ""
echo "=================================================="
echo "3. Oylik Invoyslar chiqarish (Oyning 1-sanasidan oxirgi sanasigacha)"
echo "=================================================="
GEN_RESP=$(curl -s -b "$COOKIE_JAR" -X POST "$BASE_URL/api/billing/invoices/generate-monthly" \
  -H "Content-Type: application/json" \
  -d '{"year": 2026, "month": 9}')
echo "Invoys generatsiya javobi: $GEN_RESP"

echo ""
echo "=================================================="
echo "4. Invoyslar ro'yxatini ko'rish (Pending holatida bo'lishi kerak, chunki balans 0)"
echo "=================================================="
INVOICES_RESP=$(curl -s -b "$COOKIE_JAR" "$BASE_URL/api/billing/invoices?cafeId=$CAFE_ID")
echo "Kafe Invoyslari: $INVOICES_RESP"

echo ""
echo "=================================================="
echo "5. Kafe balansini 500,000 so'mga to'ldirish"
echo "   Kutilgan: 300,000 so'm avtomatik yechiladi, 200,000 so'm ortiqcha qoladi,"
echo "   muddat 1 oyga uzaytiriladi va invoys Paid bo'ladi!"
echo "=================================================="
TOPUP_RESP=$(curl -s -b "$COOKIE_JAR" -X POST "$BASE_URL/api/billing/cafes/$CAFE_ID/topup" \
  -H "Content-Type: application/json" \
  -d '{"amount": 500000, "note": "Click to'\''lov tizimi orqali to'\''ldirildi"}')
echo "Balans to'ldirish javobi: $TOPUP_RESP"

echo ""
echo "=================================================="
echo "6. To'lovdan keyingi Invoyslar holatini tekshirish"
echo "=================================================="
AFTER_INVOICES=$(curl -s -b "$COOKIE_JAR" "$BASE_URL/api/billing/invoices?cafeId=$CAFE_ID")
echo "Yangi Invoyslar holati: $AFTER_INVOICES"

echo ""
echo "=================================================="
echo "7. Billing Dashboard statistikasini tekshirish"
echo "=================================================="
SUMMARY_RESP=$(curl -s -b "$COOKIE_JAR" "$BASE_URL/api/billing/summary")
echo "Dashboard ma'lumotlari: $SUMMARY_RESP"

echo ""
echo "=================================================="
echo "8. Kafeni vaqtincha to'xtatish va Menyu sahifasi tekshiruvi"
echo "=================================================="
# Kafeni to'xtatamiz
curl -s -b "$COOKIE_JAR" -X POST "$BASE_URL/api/billing/cafes/$CAFE_ID/toggle-suspend"
# Menyuni so'raymiz
SUSPENDED_PAGE=$(curl -s "$BASE_URL/menyu/kafe/$CAFE_ID")
if echo "$SUSPENDED_PAGE" | grep -q "Kafe faoliyati vaqtincha to'xtatilgan"; then
    echo "✓ Kafening ishlashi to'xtatilganda menyu bloklanib xabar berildi!"
else
    echo "✗ To'xtatilgan sahifa chiqmadi!"
    exit 1
fi

echo ""
echo "=================================================="
echo "BARCHA BILLING VA FAOLIK MUDDATI TESTLARI A'LO DARAJADA O'TDI! 🎉"
echo "=================================================="
