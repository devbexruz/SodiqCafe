import 'package:flutter/material.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/core/widgets/glass_container.dart';
import 'package:sellermobile/features/queue/presentation/pages/serve_customer_page.dart';

class QueuePage extends StatelessWidget {
  const QueuePage({super.key});

  @override
  Widget build(BuildContext context) {
    // Mock data for demonstration
    final List<Map<String, dynamic>> mockQueue = [
      {'id': 1, 'number': '24', 'username': 'ahmad_77', 'time': '12:45'},
      {'id': 2, 'number': '25', 'username': 'kamola_uz', 'time': '12:48'},
      {'id': 3, 'number': '26', 'username': 'shox_001', 'time': '12:51'},
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
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '🛎 Navbat (Buyurtmalar)',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                          ),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'Kutayotgan mijozlar',
                          style: TextStyle(color: Colors.black54, fontSize: 13),
                        ),
                      ],
                    ),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppTheme.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Icon(Icons.qr_code_scanner, color: AppTheme.primary, size: 28),
                    ),
                  ],
                ),
              ),
            ),
            Expanded(
              child: mockQueue.isEmpty
                  ? _buildEmptyState()
                  : GridView.builder(
                      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
                      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: 2, // Web uchun kenglikka qarab o'zgartirish mumkin
                        crossAxisSpacing: 16.0,
                        mainAxisSpacing: 16.0,
                        childAspectRatio: 0.8,
                      ),
                      itemCount: mockQueue.length,
                      itemBuilder: (context, index) {
                        return _buildQueueCard(context, mockQueue[index]);
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
          Icon(Icons.people_outline, size: 64, color: Colors.black26),
          SizedBox(height: 16),
          Text(
            'Hozircha navbatda hech kim yo\'q',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.black54),
          ),
        ],
      ),
    );
  }

  Widget _buildQueueCard(BuildContext context, Map<String, dynamic> item) {
    return Card(
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      elevation: 0,
      color: Colors.white,
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              '#${item['number']}',
              style: const TextStyle(
                fontSize: 48,
                fontWeight: FontWeight.w900,
                color: AppTheme.primary,
              ),
            ),
            const SizedBox(height: 12),
            Text(
              item['username'],
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.black87,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Kelgan vaqti: ${item['time']}',
              style: const TextStyle(
                fontSize: 12,
                color: Colors.black54,
              ),
            ),
            const Spacer(),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => ServeCustomerPage(customerData: item),
                    ),
                  );
                },
                icon: const Icon(Icons.play_circle_fill, color: Colors.white),
                label: const Text('Xizmat', style: TextStyle(fontWeight: FontWeight.bold)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
                  elevation: 2,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
