#!/bin/bash

# ==========================================
# SodiqCafe Telegram Webhook & Tunnel Script (Faqat System Bot uchun)
# ==========================================

SERVER_USER="root"
SERVER_IP="45.138.158.199"
DOMAIN="https://sodiqcafe.uz"
TELEGRAM_BOT_TOKEN="8939348394:AAE8VQ8YhIql7ESpNnpkFoXw4ebUKVAUryw" # Asosiy tizim botining tokeni
SECRET_TOKEN="AAE8VQ8YhIql7ESpNnpkFoXw4ebUKVAUryw" # appsettings.json dagi WebhookSecretToken bilan bir xil bo'lishi shart

# 0. VPS dagi band portni avtomatik tozalash
echo "0. VPS serverdagi 8080-port tekshirilmoqda va tozalanmoqda..."
ssh ${SERVER_USER}@${SERVER_IP} "fuser -k 8080/tcp 2>/dev/null || lsof -ti:8080 | xargs kill -9 2>/dev/null || true"
sleep 2
echo "----------------------------------------"

# 1. SSH orqali Reverse Tunnel ochish
echo "1. Serverga (port 8080) tunnel ochilmoqda..."
echo "Tunnelni yopish uchun CTRL+C bosing."

# SSH tunnelni orqa fonda ishga tushirish
ssh -N -R 8080:localhost:8080 ${SERVER_USER}@${SERVER_IP} &
SSH_PID=$!

# Tunnel to'liq ochilishi uchun 3 soniya kutamiz
sleep 3
echo "----------------------------------------"

# 2. Telegram Webhookni sozlash (System Bot)
echo "2. Asosiy System Bot uchun Telegram Webhook sozlanmoqda..."
# Yangilangan yo'nalish: /api/systemwebhook
WEBHOOK_URL="${DOMAIN}/api/systemwebhook"

# Xavfsizlik uchun URL va maxfiy tokenni yuboramiz
RESPONSE=$(curl -s "https://api.telegram.org/bot${TELEGRAM_BOT_TOKEN}/setWebhook?url=${WEBHOOK_URL}&secret_token=${SECRET_TOKEN}")
echo "Webhook javobi: $RESPONSE"
echo "----------------------------------------"

# CTRL+C bosilganda orqa fondagi SSH jarayonini ham to'xtatish
trap "echo -e '\nTunnel yopilmoqda... Dastur tugadi.'; kill $SSH_PID; exit" INT TERM

# Skriptni ochiq qoldirish (tunnel yopilmaguncha)
wait $SSH_PID