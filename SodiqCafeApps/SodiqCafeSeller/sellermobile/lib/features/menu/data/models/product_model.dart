class ProductModel {
  final int id;
  final String name;
  final double price;
  final String? description;
  final int? category;
  final String? imageUrl;
  final bool isRecommended;

  ProductModel({
    required this.id,
    required this.name,
    required this.price,
    this.description,
    this.category,
    this.imageUrl,
    required this.isRecommended,
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) {
    return ProductModel(
      id: json['id'],
      name: json['name'],
      price: (json['price'] as num).toDouble(),
      description: json['description'],
      category: json['category'],
      imageUrl: json['imageUrl'],
      isRecommended: json['isRecommended'] ?? false,
    );
  }
}
