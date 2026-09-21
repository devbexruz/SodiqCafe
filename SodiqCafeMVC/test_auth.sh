#!/bin/bash
set -e

BASE_URL="http://localhost:5210"
COOKIE_JAR="/tmp/cookies.txt"
rm -f "$COOKIE_JAR"

echo "=================================================="
echo "TEST 1: Ruxsatsiz kafe egasini ro'yxatdan o'tkazish (Kutilgan: 401 Unauthorized)"
echo "=================================================="
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/auth/register-cafe-owner" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "hacker",
    "email": "hacker@test.com",
    "password": "Password123!",
    "fullName": "Fake Admin",
    "cafeName": "Fake Cafe",
    "cafeAddress": "Nowhere"
  }')
echo "Natija HTTP kodi: $HTTP_CODE"
if [ "$HTTP_CODE" -eq 401 ]; then
    echo "✓ Test 1 Muvaffaqiyatli: Ruxsatsiz ro'yxatdan o'tkazish bloklandi!"
else
    echo "✗ Test 1 Xatolik: $HTTP_CODE qaytdi"
    exit 1
fi

echo ""
echo "=================================================="
echo "TEST 2: Admin orqali tizimga kirish (Login) va Cookie larni tekshirish"
echo "=================================================="
LOGIN_RESP_HEADERS="/tmp/login_headers.txt"
LOGIN_RESP_BODY="/tmp/login_body.txt"

curl -s -D "$LOGIN_RESP_HEADERS" -o "$LOGIN_RESP_BODY" -c "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "admin",
    "password": "Admin123!"
  }'

echo "Server javobi:"
cat "$LOGIN_RESP_BODY"
echo ""
echo ""
echo "Set-Cookie sarlavhalari (HttpOnly cookie tekshiruvi):"
grep -i "set-cookie" "$LOGIN_RESP_HEADERS"

# Tekshirish: X-Access-Token, X-Refresh-Token, X-Session-Token
if grep -q "X-Access-Token" "$LOGIN_RESP_HEADERS" && grep -q "httponly" "$LOGIN_RESP_HEADERS"; then
    echo "✓ X-Access-Token HttpOnly cookie da mavjud!"
else
    echo "✗ X-Access-Token topilmadi!"
    exit 1
fi

if grep -q "X-Refresh-Token" "$LOGIN_RESP_HEADERS" && grep -q "httponly" "$LOGIN_RESP_HEADERS"; then
    echo "✓ X-Refresh-Token HttpOnly cookie da mavjud!"
else
    echo "✗ X-Refresh-Token topilmadi!"
    exit 1
fi

if grep -q "X-Session-Token" "$LOGIN_RESP_HEADERS" && grep -q "httponly" "$LOGIN_RESP_HEADERS"; then
    echo "✓ X-Session-Token HttpOnly cookie da mavjud!"
else
    echo "✗ X-Session-Token topilmadi!"
    exit 1
fi

echo ""
echo "=================================================="
echo "TEST 3: Admin tomonidan yangi Kafe va uning egasini ro'yxatdan o'tkazish"
echo "=================================================="
REGISTER_BODY="/tmp/register_body.txt"
REGISTER_CODE=$(curl -s -o "$REGISTER_BODY" -w "%{http_code}" -b "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/register-cafe-owner" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "sardor_cafe",
    "email": "sardor@sodiqcafe.uz",
    "password": "Password123!",
    "fullName": "Sardor Alimov",
    "phoneNumber": "+998901112233",
    "cafeName": "Sodiq Cafe Chilonzor",
    "cafeAddress": "Chilonzor tumani, Qatortol ko'\''chasi, 15",
    "cafePhone": "+998712001122"
  }')

echo "Natija HTTP kodi: $REGISTER_CODE"
cat "$REGISTER_BODY"
echo ""
if [ "$REGISTER_CODE" -eq 200 ]; then
    echo "✓ Test 3 Muvaffaqiyatli: Admin yangi kafe va uning egasini ro'yxatdan o'tkazdi!"
else
    echo "✗ Test 3 Xatolik: $REGISTER_CODE"
    exit 1
fi

echo ""
echo "=================================================="
echo "TEST 4: Yangi ro'yxatdan o'tgan Kafe egasining login qilishi"
echo "=================================================="
CAFE_COOKIE_JAR="/tmp/cafe_cookies.txt"
CAFE_RESP_BODY="/tmp/cafe_login_body.txt"
CAFE_HEADERS="/tmp/cafe_login_headers.txt"

curl -s -D "$CAFE_HEADERS" -o "$CAFE_RESP_BODY" -c "$CAFE_COOKIE_JAR" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "usernameOrEmail": "sardor_cafe",
    "password": "Password123!"
  }'

echo "Kafe egasining javobi:"
cat "$CAFE_RESP_BODY"
echo ""

# Kafe egasi boshqa kafe egasini ro'yxatdan o'tkaza olmasligini tekshirish (Faqat Admin mumkin)
FORBIDDEN_CODE=$(curl -s -o /dev/null -w "%{http_code}" -b "$CAFE_COOKIE_JAR" -X POST "$BASE_URL/api/auth/register-cafe-owner" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "another_owner",
    "email": "another@test.uz",
    "password": "Password123!",
    "fullName": "Another Owner",
    "cafeName": "Another Cafe",
    "cafeAddress": "Address"
  }')

echo "Kafe egasi yana bitta kafe egasini qo'shishga uringandagi HTTP kodi: $FORBIDDEN_CODE"
if [ "$FORBIDDEN_CODE" -eq 403 ]; then
    echo "✓ Test 4 Muvaffaqiyatli: Kafe egasiga rad javobi berildi (403 Forbidden)! Faqat Admin ruxsatiga ega."
else
    echo "✗ Test 4 Xatolik: Kutilgan 403, lekin $FORBIDDEN_CODE qaytdi"
    exit 1
fi

echo ""
echo "=================================================="
echo "TEST 5: Token Refresh (Sessiyani yangilash)"
echo "=================================================="
REFRESH_BODY="/tmp/refresh_body.txt"
REFRESH_CODE=$(curl -s -o "$REFRESH_BODY" -w "%{http_code}" -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/refresh")
echo "Refresh HTTP kodi: $REFRESH_CODE"
cat "$REFRESH_BODY"
echo ""
if [ "$REFRESH_CODE" -eq 200 ]; then
    echo "✓ Test 5 Muvaffaqiyatli: Refresh token va Session token orqali yangi Access token olindi!"
else
    echo "✗ Test 5 Xatolik: $REFRESH_CODE"
    exit 1
fi

echo ""
echo "=================================================="
echo "TEST 6: Tizimdan chiqish (Logout) va Sessiyani bekor qilish"
echo "=================================================="
LOGOUT_CODE=$(curl -s -o /dev/null -w "%{http_code}" -b "$COOKIE_JAR" -X POST "$BASE_URL/api/auth/logout")
echo "Logout HTTP kodi: $LOGOUT_CODE"
if [ "$LOGOUT_CODE" -eq 200 ]; then
    echo "✓ Test 6 Muvaffaqiyatli: Sessiya bekor qilindi va tizimdan chiqildi!"
else
    echo "✗ Test 6 Xatolik: $LOGOUT_CODE"
    exit 1
fi

echo ""
echo "=================================================="
echo "BARCHA TESTLAR MUVAFFAQIYATLI O'TDI! 🎉"
echo "=================================================="
