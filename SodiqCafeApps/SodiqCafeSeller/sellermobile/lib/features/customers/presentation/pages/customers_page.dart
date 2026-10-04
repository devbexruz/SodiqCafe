import 'package:flutter/material.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/core/widgets/glass_container.dart';

class CustomersPage extends StatelessWidget {
  const CustomersPage({super.key});

  @override
  Widget build(BuildContext context) {
    // Mock data for demonstration
    final List<Map<String, dynamic>> mockCustomers = [
      {'name': 'Umidjon', 'phone': '+998901234567', 'purchases': 12, 'lastActivity': 'Bugun, 14:30'},
      {'name': 'Sardor', 'phone': '+998939876543', 'purchases': 8, 'lastActivity': 'Kecha, 18:15'},
      {'name': 'Malika', 'phone': '+998977778899', 'purchases': 25, 'lastActivity': '24-Sen, 09:40'},
    ];

    return Scaffold(
      backgroundColor: AppTheme.background,
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: GlassContainer(
                padding: const EdgeInsets.all(20),
                child: const Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '🧑‍🤝‍🧑 Mijozlar Bazasi',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                          ),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'Barcha mijozlaringiz ro\'yxati va tahlili',
                          style: TextStyle(color: Colors.black54, fontSize: 13),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            Expanded(
              child: mockCustomers.isEmpty
                  ? _buildEmptyState()
                  : ListView.builder(
                      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
                      itemCount: mockCustomers.length,
                      itemBuilder: (context, index) {
                        return _buildCustomerCard(mockCustomers[index]);
                      },
                    ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text('👥', style: TextStyle(fontSize: 64)),
          SizedBox(height: 16),
          Text(
            'Hozircha mijozlar bazasi bo\'sh',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.black54),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomerCard(Map<String, dynamic> customer) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      elevation: 0,
      color: Colors.white,
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Row(
          children: [
            Container(
              width: 50,
              height: 50,
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  colors: [Color(0xFF667eea), Color(0xFF764ba2)],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                shape: BoxShape.circle,
              ),
              alignment: Alignment.center,
              child: Text(
                customer['name'].substring(0, 1),
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: Colors.white),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    customer['name'],
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.black87),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    customer['phone'],
                    style: const TextStyle(fontSize: 13, color: Colors.black54),
                  ),
                ],
              ),
            ),
            Column(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppTheme.primary,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    '${customer['purchases']} ta',
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.white),
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  customer['lastActivity'],
                  style: const TextStyle(fontSize: 11, fontWeight: FontWeight.bold, color: Colors.black54),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
