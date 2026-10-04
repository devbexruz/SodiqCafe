import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/menu/data/models/product_model.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';

class ProductFormPage extends StatefulWidget {
  final ProductModel? product; // null = add, non-null = edit
  final int cafeId;

  const ProductFormPage({super.key, this.product, required this.cafeId});

  @override
  State<ProductFormPage> createState() => _ProductFormPageState();
}

class _ProductFormPageState extends State<ProductFormPage> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _nameController;
  late TextEditingController _priceController;
  late TextEditingController _descController;
  late int _selectedCategory;
  late bool _isRecommended;
  bool _isSaving = false;

  bool get _isEditing => widget.product != null;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.product?.name ?? '');
    _priceController = TextEditingController(
      text: widget.product != null ? widget.product!.price.toStringAsFixed(0) : '',
    );
    _descController = TextEditingController(text: widget.product?.description ?? '');
    _selectedCategory = widget.product?.category ?? 1;
    _isRecommended = widget.product?.isRecommended ?? false;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _priceController.dispose();
    _descController.dispose();
    super.dispose();
  }

  Color _categoryColor(int cat) {
    switch (cat) {
      case 1: return const Color(0xFF2196F3);
      case 2: return const Color(0xFFE91E63);
      case 3: return const Color(0xFF4CAF50);
      default: return AppTheme.primary;
    }
  }

  String _categoryName(int cat) {
    switch (cat) {
      case 1: return '☕ Ichimliklar';
      case 2: return '🍰 Shirinliklar';
      case 3: return '🍽 Taomlar';
      default: return 'Boshqa';
    }
  }

  void _save() {
    if (!_formKey.currentState!.validate()) return;
    final price = double.tryParse(_priceController.text) ?? 0.0;
    final messenger = ScaffoldMessenger.of(context);
    final menuBloc = context.read<MenuBloc>();
    final nav = Navigator.of(context);

    setState(() => _isSaving = true);

    final product = ProductModel(
      id: widget.product?.id ?? 0,
      name: _nameController.text.trim(),
      price: price,
      description: _descController.text.trim(),
      category: _selectedCategory,
      isRecommended: _isRecommended,
      imageUrl: widget.product?.imageUrl,
    );

    if (_isEditing) {
      menuBloc.add(EditProductEvent(product: product, cafeId: widget.cafeId));
    } else {
      menuBloc.add(AddCustomProductEvent(product: product, cafeId: widget.cafeId));
    }

    nav.pop();
    messenger.showSnackBar(
      SnackBar(
        content: Row(children: [
          const Icon(Icons.check_circle, color: Colors.white),
          const SizedBox(width: 8),
          Text(_isEditing ? 'Mahsulot yangilandi!' : 'Mahsulot qo\'shildi!'),
        ]),
        backgroundColor: Colors.green,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final primaryColor = _categoryColor(_selectedCategory);

    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        title: Text(
          _isEditing ? 'Mahsulotni Tahrirlash' : 'Yangi Mahsulot',
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        backgroundColor: Colors.white,
        foregroundColor: Colors.black87,
        elevation: 0,
        centerTitle: true,
        actions: [
          if (_isEditing)
            Padding(
              padding: const EdgeInsets.only(right: 8),
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: IconButton(
                  icon: const Icon(Icons.delete_outline, color: Colors.red),
                  onPressed: () {
                    // Delete is handled from the list page
                    Navigator.pop(context, 'delete');
                  },
                ),
              ),
            ),
        ],
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            // Category selector
            const Text(
              'Kategoriya',
              style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: Colors.black54),
            ),
            const SizedBox(height: 8),
            Row(
              children: [1, 2, 3].map((cat) {
                final isSelected = _selectedCategory == cat;
                final color = _categoryColor(cat);
                return Expanded(
                  child: GestureDetector(
                    onTap: () => setState(() => _selectedCategory = cat),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      margin: const EdgeInsets.only(right: 8),
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      decoration: BoxDecoration(
                        color: isSelected ? color : Colors.white,
                        borderRadius: BorderRadius.circular(14),
                        border: Border.all(
                          color: isSelected ? color : Colors.grey.shade200,
                          width: isSelected ? 2 : 1,
                        ),
                        boxShadow: isSelected
                            ? [BoxShadow(color: color.withOpacity(0.3), blurRadius: 8, offset: const Offset(0, 4))]
                            : [],
                      ),
                      child: Text(
                        _categoryName(cat),
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          color: isSelected ? Colors.white : Colors.black54,
                          fontSize: 12,
                          fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
                        ),
                      ),
                    ),
                  ),
                );
              }).toList(),
            ),
            const SizedBox(height: 24),

            // Name field
            _buildLabel('Mahsulot nomi'),
            const SizedBox(height: 8),
            TextFormField(
              controller: _nameController,
              textCapitalization: TextCapitalization.sentences,
              decoration: _inputDecoration('Masalan: Cappuccino', primaryColor, Icons.restaurant_menu),
              validator: (val) {
                if (val == null || val.trim().isEmpty) return 'Nomi kiritilishi shart';
                return null;
              },
            ),
            const SizedBox(height: 20),

            // Price field
            _buildLabel('Narxi (UZS)'),
            const SizedBox(height: 8),
            TextFormField(
              controller: _priceController,
              keyboardType: TextInputType.number,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              decoration: _inputDecoration('0', primaryColor, Icons.attach_money).copyWith(
                suffixText: 'UZS',
                suffixStyle: const TextStyle(fontWeight: FontWeight.bold, color: Colors.black54),
              ),
              validator: (val) {
                if (val == null || val.isEmpty) return 'Narx kiritilishi shart';
                final p = double.tryParse(val);
                if (p == null || p <= 0) return 'To\'g\'ri narx kiriting';
                return null;
              },
            ),
            const SizedBox(height: 20),

            // Description field
            _buildLabel('Tavsif (ixtiyoriy)'),
            const SizedBox(height: 8),
            TextFormField(
              controller: _descController,
              maxLines: 3,
              decoration: _inputDecoration('Mahsulot haqida qisqacha...', primaryColor, Icons.notes),
            ),
            const SizedBox(height: 20),

            // Recommended toggle
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
                      color: Colors.amber.shade50,
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Icons.star, color: Colors.amber, size: 22),
                  ),
                  const SizedBox(width: 12),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Tavsiya etilsin', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                        Text('Menyu sahifasida tavsiya belgisi ko\'rsatiladi', style: TextStyle(color: Colors.black45, fontSize: 12)),
                      ],
                    ),
                  ),
                  Switch(
                    value: _isRecommended,
                    onChanged: (val) => setState(() => _isRecommended = val),
                    activeColor: Colors.amber,
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
                  backgroundColor: primaryColor,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  elevation: 0,
                ),
                child: _isSaving
                    ? const SizedBox(
                        width: 24, height: 24,
                        child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2.5),
                      )
                    : Text(
                        _isEditing ? 'Saqlash' : 'Qo\'shish',
                        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
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

  Widget _buildLabel(String text) {
    return Text(
      text,
      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: Colors.black54),
    );
  }

  InputDecoration _inputDecoration(String hint, Color color, IconData icon) {
    return InputDecoration(
      hintText: hint,
      hintStyle: const TextStyle(color: Colors.black26),
      prefixIcon: Icon(icon, color: color, size: 20),
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: BorderSide(color: Colors.grey.shade200),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: BorderSide(color: Colors.grey.shade200),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: BorderSide(color: color, width: 2),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: Colors.red),
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
    );
  }
}
