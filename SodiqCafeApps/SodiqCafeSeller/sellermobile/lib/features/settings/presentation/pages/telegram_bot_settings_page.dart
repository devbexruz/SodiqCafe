import 'package:flutter/material.dart';
import 'package:sellermobile/config/theme/app_theme.dart';

class TelegramBotSettingsPage extends StatelessWidget {
  const TelegramBotSettingsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        title: const Text('Telegram Bot Sozlamalari', style: TextStyle(color: Colors.black87, fontSize: 18, fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.black87),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Telegram botni ulash',
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 12),
            const Text(
              'Bu yerda siz o\'z kafengiz uchun Telegram bot tokenini kiritishingiz va botni ishga tushirishingiz mumkin. Bu funksiya orqali mijozlar bot orqali buyurtma berish imkoniyatiga ega bo\'ladilar.',
              style: TextStyle(color: Colors.black54, height: 1.5),
            ),
            const SizedBox(height: 32),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Bot Token',
                hintText: 'Masalan: 123456789:ABCdefGhIJKlmNoPQRstuVWXyz',
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 24),
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton(
                onPressed: () {
                  // TODO: Save bot token and integrate
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Bot tokeni saqlandi. (Hali to\'liq ishga tushmadi)')),
                  );
                },
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primary,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                child: const Text('Saqlash va Ulash', style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
