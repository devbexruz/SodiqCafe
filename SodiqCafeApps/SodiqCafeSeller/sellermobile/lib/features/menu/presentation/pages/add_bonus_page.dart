import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/menu/data/models/bonus_campaign_model.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';

class AddBonusPage extends StatefulWidget {
  final int cafeId;

  const AddBonusPage({super.key, required this.cafeId});

  @override
  State<AddBonusPage> createState() => _AddBonusPageState();
}

class _AddBonusPageState extends State<AddBonusPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descController = TextEditingController();
  final _conditionController = TextEditingController();
  final _rewardController = TextEditingController();
  bool _isVisible = true;
  bool _isSaving = false;

  @override
  void dispose() {
    _nameController.dispose();
    _descController.dispose();
    _conditionController.dispose();
    _rewardController.dispose();
    super.dispose();
  }

  void _save() {
    if (!_formKey.currentState!.validate()) return;
    final condition = double.tryParse(_conditionController.text) ?? 0;
    final messenger = ScaffoldMessenger.of(context);
    final menuBloc = context.read<MenuBloc>();
    final nav = Navigator.of(context);

    setState(() => _isSaving = true);

    final bonus = BonusCampaignModel(
      id: 0,
      cafeId: widget.cafeId,
      name: _nameController.text.trim(),
      description: _descController.text.trim(),
      type: 0,
      conditionValue: condition,
      rewardDescription: _rewardController.text.trim(),
      isActive: true,
      isVisible: _isVisible,
    );

    menuBloc.add(AddBonusCampaignEvent(campaign: bonus, cafeId: widget.cafeId));

    nav.pop();
    messenger.showSnackBar(
      SnackBar(
        content: const Row(children: [
          Icon(Icons.star, color: Colors.white),
          SizedBox(width: 8),
          Text('Aksiya muvaffaqiyatli yaratildi!'),
        ]),
        backgroundColor: AppTheme.primary,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        title: const Text(
          'Yangi Aksiya',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
        elevation: 0,
        centerTitle: true,
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            // Header banner
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [AppTheme.primary, AppTheme.primaryDark],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(20),
              ),
              child: const Row(
                children: [
                  Icon(Icons.card_giftcard, color: Colors.white, size: 40),
                  SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Yangi aksiya yarating',
                          style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'Mijozlaringizni rag\'batlantiring va sodiqligini oshiring',
                          style: TextStyle(color: Colors.white70, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Name
            _buildSection(
              title: 'Aksiya Ma\'lumotlari',
              icon: Icons.campaign_outlined,
              children: [
                _buildLabel('Aksiya nomi *'),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _nameController,
                  textCapitalization: TextCapitalization.sentences,
                  decoration: _inputDecoration('Masalan: 5ta kofe - 1 bepul!', AppTheme.primary, Icons.label_outline),
                  validator: (val) => (val == null || val.trim().isEmpty) ? 'Nomi kiritilishi shart' : null,
                ),
                const SizedBox(height: 16),
                _buildLabel('Tavsif (ixtiyoriy)'),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _descController,
                  maxLines: 2,
                  decoration: _inputDecoration('Aksiya haqida qo\'shimcha ma\'lumot...', AppTheme.primary, Icons.notes_outlined),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Condition
            _buildSection(
              title: 'Shart',
              icon: Icons.flag_outlined,
              children: [
                _buildLabel('Nechanchi xariddan keyin? *'),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _conditionController,
                  keyboardType: TextInputType.number,
                  style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  decoration: _inputDecoration('5', AppTheme.primary, Icons.shopping_bag_outlined).copyWith(
                    suffixText: 'xarid',
                    suffixStyle: const TextStyle(fontWeight: FontWeight.bold, color: Colors.black45),
                    helperText: 'Masalan: 5 ta xariddan keyin bonus beriladi',
                  ),
                  validator: (val) {
                    if (val == null || val.isEmpty) return 'Shart kiritilishi shart';
                    final n = double.tryParse(val);
                    if (n == null || n <= 0) return 'To\'g\'ri son kiriting';
                    return null;
                  },
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Reward
            _buildSection(
              title: 'Mukofot',
              icon: Icons.card_giftcard_outlined,
              children: [
                _buildLabel('Mijoz nima oladi? *'),
                const SizedBox(height: 8),
                TextFormField(
                  controller: _rewardController,
                  maxLines: 2,
                  textCapitalization: TextCapitalization.sentences,
                  decoration: _inputDecoration('Masalan: 1 stakan bepul Americano', Colors.amber.shade700, Icons.redeem_outlined),
                  validator: (val) => (val == null || val.trim().isEmpty) ? 'Mukofot kiritilishi shart' : null,
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Visibility
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: Colors.grey.shade200),
              ),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(8),
                    decoration: BoxDecoration(
                      color: Colors.blue.shade50,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Icon(
                      _isVisible ? Icons.visibility : Icons.visibility_off,
                      color: Colors.blue.shade600,
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Mijozlarga ko\'rinadimi?', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                        Text(
                          _isVisible ? 'Mijozlar bu aksiyani ko\'radi' : 'Faqat siz ko\'rasiz',
                          style: const TextStyle(color: Colors.black45, fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                  Switch(
                    value: _isVisible,
                    onChanged: (val) => setState(() => _isVisible = val),
                    activeColor: AppTheme.primary,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Save button
            SizedBox(
              width: double.infinity,
              height: 56,
              child: ElevatedButton(
                onPressed: _isSaving ? null : _save,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primary,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  elevation: 0,
                ),
                child: _isSaving
                    ? const SizedBox(
                        width: 24, height: 24,
                        child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5),
                      )
                    : const Text(
                        'Aksiyani Yaratish',
                        style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                      ),
              ),
            ),
            const SizedBox(height: 12),
            SizedBox(
              width: double.infinity,
              height: 50,
              child: TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Bekor qilish', style: TextStyle(color: Colors.black45, fontSize: 15)),
              ),
            ),
            const SizedBox(height: 20),
          ],
        ),
      ),
    );
  }

  Widget _buildSection({required String title, required IconData icon, required List<Widget> children}) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.grey.shade100),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, color: AppTheme.primary, size: 20),
              const SizedBox(width: 8),
              Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
            ],
          ),
          const Divider(height: 20),
          ...children,
        ],
      ),
    );
  }

  Widget _buildLabel(String text) {
    return Text(text, style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: Colors.black54));
  }

  InputDecoration _inputDecoration(String hint, Color color, IconData icon) {
    return InputDecoration(
      hintText: hint,
      hintStyle: const TextStyle(color: Colors.black26),
      prefixIcon: Icon(icon, color: color, size: 20),
      filled: true,
      fillColor: const Color(0xFFF8F9FA),
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: BorderSide.none),
      enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: BorderSide.none),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: BorderSide(color: color, width: 2),
      ),
      errorBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(14), borderSide: const BorderSide(color: Colors.red)),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
    );
  }
}
