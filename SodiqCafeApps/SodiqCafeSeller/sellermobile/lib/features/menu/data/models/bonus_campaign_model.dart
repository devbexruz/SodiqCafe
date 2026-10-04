class BonusCampaignModel {
  final int id;
  final int cafeId;
  final String name;
  final String? description;
  final int type;
  final double conditionValue;
  final String rewardDescription;
  final bool isActive;
  final bool isVisible;

  BonusCampaignModel({
    required this.id,
    required this.cafeId,
    required this.name,
    this.description,
    required this.type,
    required this.conditionValue,
    required this.rewardDescription,
    required this.isActive,
    required this.isVisible,
  });

  factory BonusCampaignModel.fromJson(Map<String, dynamic> json) {
    return BonusCampaignModel(
      id: json['id'],
      cafeId: json['cafeId'],
      name: json['name'],
      description: json['description'],
      type: json['type'] ?? 0,
      conditionValue: (json['conditionValue'] ?? 0).toDouble(),
      rewardDescription: json['rewardDescription'] ?? '',
      isActive: json['isActive'] ?? false,
      isVisible: json['isVisible'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'cafeId': cafeId,
      'name': name,
      'description': description,
      'type': type,
      'conditionValue': conditionValue,
      'rewardDescription': rewardDescription,
      'isActive': isActive,
      'isVisible': isVisible,
    };
  }
}
