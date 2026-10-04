import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/menu/data/models/product_model.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';

class AddFromTemplatePage extends StatefulWidget {
  final ProductModel template;

  const AddFromTemplatePage({super.key, required this.template});

  @override
  State<AddFromTemplatePage> createState() => _AddFromTemplatePageState();
}

class _AddFromTemplatePageState extends State<AddFromTemplatePage> {
  late TextEditingController _priceController;
  final _formKey = GlobalKey<FormState>();
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _priceController = TextEditingController(
      text: widget.template.price.toStringAsFixed(0),
    );
  }

  @override
  void dispose() {
    _priceController.dispose();
    super.dispose();
  }

  String _categoryName(int? cat) {
    switch (cat) {
      case 1: return 'Ichimliklar';
      case 2: return 'Shirinliklar';
      case 3: return 'Taomlar';
      default: return 'Boshqa';
    }
  }

  Color _categoryColor(int? cat) {
    switch (cat) {
      case 1: return const Color(0xFF2196F3);
      case 2: return const Color(0xFFE91E63);
      case 3: return const Color(0xFF4CAF50);
      default: return AppTheme.primary;
    }
  }

  void _save() {
    if (!_formKey.currentState!.validate()) return;
    final price = double.tryParse(_priceController.text) ?? widget.template.price;
    final dashState = context.read<DashboardCubit>().state;
    if (dashState is! DashboardLoaded) return;
    final cafeId = dashState.cafes.id;
    final messenger = ScaffoldMessenger.of(context);
    final menuBloc = context.read<MenuBloc>();
    final nav = Navigator.of(context);

    setState(() => _isSaving = true);
    menuBloc.add(AddProductFromTemplateEvent(
      templateId: widget.template.id,
      customPrice: price,
      cafeId: cafeId,
    ));
    nav.pop();
    messenger.showSnackBar(
      SnackBar(
        content: Row(children: [
          const Icon(Icons.check_circle, color: Colors.white),
          const SizedBox(width: 8),
          Text('${widget.template.name} menyuga qo\'shildi!'),
        ]),
        backgroundColor: Colors.green,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final template = widget.template;
    final catColor = _categoryColor(template.category);

    return Scaffold(
      backgroundColor: AppTheme.background,
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            expandedHeight: 200,
            pinned: true,
            backgroundColor: catColor,
            foregroundColor: Colors.white,
            flexibleSpace: FlexibleSpaceBar(
              background: Stack(
                fit: StackFit.expand,
                children: [
                  Container(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        colors: [catColor, catColor.withOpacity(0.7)],
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                      ),
                    ),
                  ),
                  if (template.imageUrl != null && template.imageUrl!.isNotEmpty)
                    Opacity(
                      opacity: 0.3,
                      child: Image.network(template.imageUrl!, fit: BoxFit.cover),
                    ),
                  Positioned(
                    bottom: 20,
                    left: 16,
                    right: 16,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(
                          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.2),
                            borderRadius: BorderRadius.circular(20),
                          ),
                          child: Text(
                            _categoryName(template.category),
                            style: const TextStyle(color: Colors.white, fontSize: 12),
                          ),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          template.name,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 26,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        if (template.description != null && template.description!.isNotEmpty)
                          Text(
                            template.description!,
                            style: const TextStyle(color: Colors.white70, fontSize: 13),
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                          ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
          SliverToBoxAdapter(
            child: Padding(
              padding: const EdgeInsets.all(20),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Info card
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: catColor.withOpacity(0.05),
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(color: catColor.withOpacity(0.2)),
                      ),
                      child: Row(
                        children: [
                          Icon(Icons.info_outline, color: catColor, size: 20),
                          const SizedBox(width: 10),
                          Expanded(
                            child: Text(
                              'Shablon narxi: ${template.price.toStringAsFixed(0)} UZS. Kerak bo\'lsa o\'zgartiring.',
                              style: TextStyle(color: catColor, fontSize: 13),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Price field
                    const Text(
                      'Sotiladigan narx',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const SizedBox(height: 8),
                    TextFormField(
                      controller: _priceController,
                      keyboardType: TextInputType.number,
                      style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                      decoration: InputDecoration(
                        hintText: '0',
                        suffixText: 'UZS',
                        suffixStyle: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Colors.black54,
                        ),
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
                          borderSide: BorderSide(color: catColor, width: 2),
                        ),
                        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 18),
                      ),
                      validator: (val) {
                        if (val == null || val.isEmpty) return 'Narxni kiriting';
                        final p = double.tryParse(val);
                        if (p == null || p <= 0) return 'To\'g\'ri narx kiriting';
                        return null;
                      },
                    ),
                    const SizedBox(height: 32),

                    // Save button
                    SizedBox(
                      width: double.infinity,
                      height: 56,
                      child: ElevatedButton(
                        onPressed: _isSaving ? null : _save,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: catColor,
                          foregroundColor: Colors.white,
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(16),
                          ),
                          elevation: 0,
                        ),
                        child: _isSaving
                            ? const CircularProgressIndicator(color: Colors.white)
                            : const Text(
                                'Menyuga Qo\'shish',
                                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                              ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      height: 52,
                      child: TextButton(
                        onPressed: () => Navigator.pop(context),
                        child: const Text('Bekor qilish', style: TextStyle(color: Colors.black54)),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
