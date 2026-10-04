import 'package:flutter/material.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/core/widgets/glass_container.dart';

class ServeCustomerPage extends StatelessWidget {
  final Map<String, dynamic> customerData;

  const ServeCustomerPage({super.key, required this.customerData});

  @override
  Widget build(BuildContext context) {
    // Mock campaigns data for this user
    final List<Map<String, dynamic>> campaigns = [
      {'id': 1, 'name': '10-chisi bepul!', 'reward': '1 ta Bepul Kofe', 'isRecommended': true, 'reason': 'Doimiy mijoz (8/10)', 'type': 'stamp'},
      {'id': 2, 'name': 'Yangi Mijoz', 'reward': 'Katta chegirma', 'isRecommended': false, 'reason': '', 'type': 'new'},
    ];

    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.black87),
        title: const Text('Xizmat Ko\'rsatish', style: TextStyle(color: Colors.black87, fontWeight: FontWeight.bold)),
        centerTitle: true,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Mijoz haqida qisqacha
              GlassContainer(
                padding: const EdgeInsets.all(20),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Mijoz: ${customerData['username']}',
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Buyurtma raqami: #${customerData['number']}',
                          style: const TextStyle(color: AppTheme.primary, fontSize: 16, fontWeight: FontWeight.bold),
                        ),
                      ],
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: Colors.blue.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Text('Doimiy mijoz', style: TextStyle(color: Colors.blue, fontWeight: FontWeight.bold)),
                    ),
                  ],
                ),
              ),
              
              const SizedBox(height: 24),
              const Text('Xarid summasi', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              const SizedBox(height: 8),
              TextField(
                keyboardType: TextInputType.number,
                decoration: InputDecoration(
                  hintText: "0 so'm",
                  filled: true,
                  fillColor: Colors.white,
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                    borderSide: BorderSide.none,
                  ),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
                ),
              ),
              
              const SizedBox(height: 24),
              const Text('Tavsiya etilgan Aksiyalar', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
              const SizedBox(height: 12),
              
              ...campaigns.map((camp) => _buildCampaignItem(camp)),
              
              const SizedBox(height: 32),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: () {
                    Navigator.pop(context); // Qaytish
                  },
                  icon: const Icon(Icons.check_circle),
                  label: const Text('Yakunlash va Bonus berish'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.green,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildCampaignItem(Map<String, dynamic> campaign) {
    bool isRec = campaign['isRecommended'];
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: isRec ? Colors.blue.withOpacity(0.05) : Colors.white,
        border: Border.all(color: isRec ? Colors.blue.withOpacity(0.3) : Colors.transparent),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          if (!isRec) BoxShadow(color: Colors.black.withOpacity(0.02), blurRadius: 8, offset: const Offset(0, 2)),
        ],
      ),
      child: CheckboxListTile(
        value: isRec,
        onChanged: (val) {},
        activeColor: AppTheme.primary,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text(campaign['name'], style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(campaign['reward'], style: const TextStyle(fontSize: 12, color: Colors.black54)),
            if (isRec && campaign['reason'].toString().isNotEmpty) ...[
              const SizedBox(height: 4),
              Row(
                children: [
                  const Icon(Icons.check_circle, size: 12, color: Colors.green),
                  const SizedBox(width: 4),
                  Text(campaign['reason'], style: const TextStyle(fontSize: 11, color: Colors.green, fontWeight: FontWeight.bold)),
                ],
              )
            ]
          ],
        ),
      ),
    );
  }
}
