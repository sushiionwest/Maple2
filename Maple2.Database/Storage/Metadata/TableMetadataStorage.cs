using Maple2.Database.Context;
using Maple2.Model.Common;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class TableMetadataStorage {
    private Lazy<ChatStickerTable> chatStickerTable = null!;
    private Lazy<DefaultItemsTable> defaultItemsTable = null!;
    private Lazy<ItemBreakTable> itemBreakTable = null!;
    private Lazy<ItemExtractionTable> itemExtractionTable = null!;
    private Lazy<GemstoneUpgradeTable> gemstoneUpgradeTable = null!;
    private Lazy<JobTable> jobTable = null!;
    private Lazy<MagicPathTable> magicPathTable = null!;
    private Lazy<MasteryRecipeTable> masteryRecipeTable = null!;
    private Lazy<MasteryRewardTable> masteryRewardTable = null!;
    private Lazy<MasteryDifferentialFactorTable> masteryDifferentialFactorTable = null!;
    private Lazy<FishingRodTable> fishingRodTable = null!;
    private Lazy<InstrumentTable> instrumentTable = null!;
    private Lazy<InteractObjectTable> interactObjectTable = null!;
    private Lazy<LapenshardUpgradeTable> lapenshardUpgradeTable = null!;
    private Lazy<ItemSocketTable> itemSocketTable = null!;
    private Lazy<GuildTable> guildTable = null!;
    private Lazy<PremiumClubTable> premiumClubTable = null!;
    private Lazy<IndividualItemDropTable> individualItemDropTable = null!;
    private Lazy<ColorPaletteTable> colorPaletteTable = null!;
    private Lazy<MeretMarketCategoryTable> meretMarketCategoryTable = null!;
    private Lazy<ShopBeautyCouponTable> shopBeautyCouponTable = null!;
    private Lazy<FurnishingShopTable> furnishingShopTable = null!;
    private Lazy<GachaInfoTable> gachaInfoTable = null!;
    private Lazy<InsigniaTable> insigniaTable = null!;
    private Lazy<ExpTable> expTable = null!;
    private Lazy<CommonExpTable> commonExpTable = null!;
    private Lazy<UgcDesignTable> ugcDesignTable = null!;
    private Lazy<MasteryUgcHousingTable> masteryUgcHousingTable = null!;
    private Lazy<UgcHousingPointRewardTable> ugcHousingPointRewardTable = null!;
    private Lazy<LearningQuestTable> learningQuestTable = null!;
    private Lazy<PrestigeLevelAbilityTable> prestigeLevelAbilityTable = null!;
    private Lazy<PrestigeLevelRewardTable> prestigeLevelRewardTable = null!;
    private Lazy<PrestigeMissionTable> prestigeMissionTable = null!;
    private Lazy<BlackMarketTable> blackMarketTable = null!;
    private Lazy<ChangeJobTable> changeJobTable = null!;
    private Lazy<ChapterBookTable> chapterBookTable = null!;
    private Lazy<FieldMissionTable> fieldMissionTable = null!;
    private Lazy<WorldMapTable> worldMapTable = null!;
    private Lazy<SurvivalSkinInfoTable> survivalSkinInfoTable = null!;
    private Lazy<BannerTable> bannerTable = null!;
    private Lazy<WeddingTable> weddingTable = null!;
    private Lazy<RewardContentTable> rewardContentTable = null!;
    private Lazy<SeasonDataTable> seasonDataTable = null!;
    private Lazy<SmartPushTable> smartPushTable = null!;
    private Lazy<AutoActionTable> autoActionTable = null!;

    private Lazy<EnchantScrollTable> enchantScrollTable = null!;
    private Lazy<ItemRemakeScrollTable> itemRemakeScrollTable = null!;
    private Lazy<ItemRepackingScrollTable> itemRepackingScrollTable = null!;
    private Lazy<ItemSocketScrollTable> itemSocketScrollTable = null!;
    private Lazy<ItemExchangeScrollTable> itemExchangeScrollTable = null!;

    private Lazy<ItemOptionConstantTable> itemOptionConstantTable = null!;
    private Lazy<ItemOptionRandomTable> itemOptionRandomTable = null!;
    private Lazy<ItemOptionStaticTable> itemOptionStaticTable = null!;
    private Lazy<ItemOptionPickTable> itemOptionPickTable = null!;
    private Lazy<ItemVariationTable> itemVariationTable = null!;
    private Lazy<ItemEquipVariationTable> accVariationTable = null!;
    private Lazy<ItemEquipVariationTable> armorVariationTable = null!;
    private Lazy<ItemEquipVariationTable> petVariationTable = null!;
    private Lazy<ItemEquipVariationTable> weaponVariationTable = null!;

    private Lazy<DungeonRoomTable> dungeonRoomTable = null!;
    private Lazy<DungeonRankRewardTable> dungeonRankRewardTable = null!;
    private Lazy<DungeonConfigTable> dungeonConfigTable = null!;
    private Lazy<DungeonMissionTable> dungeonMissionTable = null!;

    public ChatStickerTable ChatStickerTable => chatStickerTable.Value;
    public DefaultItemsTable DefaultItemsTable => defaultItemsTable.Value;
    public ItemBreakTable ItemBreakTable => itemBreakTable.Value;
    public ItemExtractionTable ItemExtractionTable => itemExtractionTable.Value;
    public GemstoneUpgradeTable GemstoneUpgradeTable => gemstoneUpgradeTable.Value;
    public JobTable JobTable => jobTable.Value;
    public MagicPathTable MagicPathTable => magicPathTable.Value;
    public MasteryRecipeTable MasteryRecipeTable => masteryRecipeTable.Value;
    public MasteryRewardTable MasteryRewardTable => masteryRewardTable.Value;
    public MasteryDifferentialFactorTable MasteryDifferentialFactorTable => masteryDifferentialFactorTable.Value;
    public FishingRodTable FishingRodTable => fishingRodTable.Value;
    public InstrumentTable InstrumentTable => instrumentTable.Value;
    public InteractObjectTable InteractObjectTable => interactObjectTable.Value;
    public LapenshardUpgradeTable LapenshardUpgradeTable => lapenshardUpgradeTable.Value;
    public ItemSocketTable ItemSocketTable => itemSocketTable.Value;
    public GuildTable GuildTable => guildTable.Value;
    public PremiumClubTable PremiumClubTable => premiumClubTable.Value;
    public IndividualItemDropTable IndividualItemDropTable => individualItemDropTable.Value;
    public ColorPaletteTable ColorPaletteTable => colorPaletteTable.Value;
    public MeretMarketCategoryTable MeretMarketCategoryTable => meretMarketCategoryTable.Value;
    public ShopBeautyCouponTable ShopBeautyCouponTable => shopBeautyCouponTable.Value;
    public FurnishingShopTable FurnishingShopTable => furnishingShopTable.Value;
    public GachaInfoTable GachaInfoTable => gachaInfoTable.Value;
    public InsigniaTable InsigniaTable => insigniaTable.Value;
    public ExpTable ExpTable => expTable.Value;
    public CommonExpTable CommonExpTable => commonExpTable.Value;
    public UgcDesignTable UgcDesignTable => ugcDesignTable.Value;
    public MasteryUgcHousingTable MasteryUgcHousingTable => masteryUgcHousingTable.Value;
    public UgcHousingPointRewardTable UgcHousingPointRewardTable => ugcHousingPointRewardTable.Value;
    public LearningQuestTable LearningQuestTable => learningQuestTable.Value;
    public PrestigeLevelAbilityTable PrestigeLevelAbilityTable => prestigeLevelAbilityTable.Value;
    public PrestigeLevelRewardTable PrestigeLevelRewardTable => prestigeLevelRewardTable.Value;
    public PrestigeMissionTable PrestigeMissionTable => prestigeMissionTable.Value;
    public BlackMarketTable BlackMarketTable => blackMarketTable.Value;
    public ChangeJobTable ChangeJobTable => changeJobTable.Value;
    public ChapterBookTable ChapterBookTable => chapterBookTable.Value;
    public FieldMissionTable FieldMissionTable => fieldMissionTable.Value;
    public WorldMapTable WorldMapTable => worldMapTable.Value;
    public SurvivalSkinInfoTable SurvivalSkinInfoTable => survivalSkinInfoTable.Value;
    public BannerTable BannerTable => bannerTable.Value;
    public WeddingTable WeddingTable => weddingTable.Value;
    public RewardContentTable RewardContentTable => rewardContentTable.Value;
    public SeasonDataTable SeasonDataTable => seasonDataTable.Value;
    public SmartPushTable SmartPushTable => smartPushTable.Value;
    public AutoActionTable AutoActionTable => autoActionTable.Value;

    public EnchantScrollTable EnchantScrollTable => enchantScrollTable.Value;
    public ItemRemakeScrollTable ItemRemakeScrollTable => itemRemakeScrollTable.Value;
    public ItemRepackingScrollTable ItemRepackingScrollTable => itemRepackingScrollTable.Value;
    public ItemSocketScrollTable ItemSocketScrollTable => itemSocketScrollTable.Value;
    public ItemExchangeScrollTable ItemExchangeScrollTable => itemExchangeScrollTable.Value;

    public ItemOptionConstantTable ItemOptionConstantTable => itemOptionConstantTable.Value;
    public ItemOptionRandomTable ItemOptionRandomTable => itemOptionRandomTable.Value;
    public ItemOptionStaticTable ItemOptionStaticTable => itemOptionStaticTable.Value;
    public ItemOptionPickTable ItemOptionPickTable => itemOptionPickTable.Value;
    public ItemVariationTable ItemVariationTable => itemVariationTable.Value;
    public ItemEquipVariationTable AccessoryVariationTable => accVariationTable.Value;
    public ItemEquipVariationTable ArmorVariationTable => armorVariationTable.Value;
    public ItemEquipVariationTable PetVariationTable => petVariationTable.Value;
    public ItemEquipVariationTable WeaponVariationTable => weaponVariationTable.Value;

    public DungeonRoomTable DungeonRoomTable => dungeonRoomTable.Value;
    public DungeonRankRewardTable DungeonRankRewardTable => dungeonRankRewardTable.Value;
    public DungeonConfigTable DungeonConfigTable => dungeonConfigTable.Value;
    public DungeonMissionTable DungeonMissionTable => dungeonMissionTable.Value;

    private readonly MetadataContext context;

    public TableMetadataStorage(MetadataContext context) {
        this.context = context;
        Load();
    }

    /// <summary>
    /// Drops every loaded table so the next access re-reads it from the metadata database.
    /// </summary>
    public void InvalidateCache() {
        Load();
    }

    private void Load() {
        chatStickerTable = Retrieve<ChatStickerTable>(context, TableNames.CHAT_EMOTICON);
        defaultItemsTable = Retrieve<DefaultItemsTable>(context, TableNames.DEFAULT_ITEMS);
        itemBreakTable = Retrieve<ItemBreakTable>(context, TableNames.ITEM_BREAK_INGREDIENT);
        itemExtractionTable = Retrieve<ItemExtractionTable>(context, TableNames.ITEM_EXTRACTION);
        gemstoneUpgradeTable = Retrieve<GemstoneUpgradeTable>(context, TableNames.ITEM_GEMSTONE_UPGRADE);
        jobTable = Retrieve<JobTable>(context, TableNames.JOB);
        magicPathTable = Retrieve<MagicPathTable>(context, TableNames.MAGIC_PATH);
        masteryRecipeTable = Retrieve<MasteryRecipeTable>(context, TableNames.MASTERY_RECIPE);
        masteryRewardTable = Retrieve<MasteryRewardTable>(context, TableNames.MASTERY);
        masteryDifferentialFactorTable = Retrieve<MasteryDifferentialFactorTable>(context, TableNames.MASTERYT_DIFFERENTIAL_FACTOR);
        fishingRodTable = Retrieve<FishingRodTable>(context, TableNames.FISHING_ROD);
        instrumentTable = Retrieve<InstrumentTable>(context, TableNames.INSTRUMENT_CATEGORY_INFO);
        interactObjectTable = Retrieve<InteractObjectTable>(context, TableNames.INTERACT_OBJECT);
        lapenshardUpgradeTable = Retrieve<LapenshardUpgradeTable>(context, TableNames.ITEM_LAPENSHARD_UPGRADE);
        itemSocketTable = Retrieve<ItemSocketTable>(context, TableNames.ITEM_SOCKET);
        guildTable = Retrieve<GuildTable>(context, TableNames.GUILD);
        premiumClubTable = Retrieve<PremiumClubTable>(context, TableNames.VIP);
        individualItemDropTable = Retrieve<IndividualItemDropTable>(context, TableNames.INDIVIDUAL_ITEM_DROP);
        colorPaletteTable = Retrieve<ColorPaletteTable>(context, TableNames.COLOR_PALETTE);
        meretMarketCategoryTable = Retrieve<MeretMarketCategoryTable>(context, TableNames.MERET_MARKET_CATEGORY);
        shopBeautyCouponTable = Retrieve<ShopBeautyCouponTable>(context, TableNames.SHOP_BEAUTY_COUPON);
        furnishingShopTable = Retrieve<FurnishingShopTable>(context, TableNames.SHOP_FURNISHING);
        gachaInfoTable = Retrieve<GachaInfoTable>(context, TableNames.GACHA_INFO);
        insigniaTable = Retrieve<InsigniaTable>(context, TableNames.NAME_TAG_SYMBOL);
        expTable = Retrieve<ExpTable>(context, TableNames.EXP);
        commonExpTable = Retrieve<CommonExpTable>(context, TableNames.COMMON_EXP);
        ugcDesignTable = Retrieve<UgcDesignTable>(context, TableNames.UGC_DESIGN);
        masteryUgcHousingTable = Retrieve<MasteryUgcHousingTable>(context, TableNames.MASTERY_UGC_HOUSING);
        ugcHousingPointRewardTable = Retrieve<UgcHousingPointRewardTable>(context, TableNames.UGC_HOUSING_POINT_REWARD);
        learningQuestTable = Retrieve<LearningQuestTable>(context, TableNames.LEARNING_QUEST);
        prestigeLevelAbilityTable = Retrieve<PrestigeLevelAbilityTable>(context, TableNames.PRESTIGE_LEVEL_ABILITY);
        prestigeLevelRewardTable = Retrieve<PrestigeLevelRewardTable>(context, TableNames.PRESTIGE_LEVEL_REWARD);
        prestigeMissionTable = Retrieve<PrestigeMissionTable>(context, TableNames.PRESTIGE_MISSION);
        blackMarketTable = Retrieve<BlackMarketTable>(context, TableNames.BLACK_MARKET_TABLE);
        changeJobTable = Retrieve<ChangeJobTable>(context, TableNames.CHANGE_JOB);
        chapterBookTable = Retrieve<ChapterBookTable>(context, TableNames.CHAPTER_BOOK);
        fieldMissionTable = Retrieve<FieldMissionTable>(context, TableNames.FIELD_MISSION);
        worldMapTable = Retrieve<WorldMapTable>(context, TableNames.WORLD_MAP);
        survivalSkinInfoTable = Retrieve<SurvivalSkinInfoTable>(context, TableNames.SURVIVAL_SKIN_INFO);
        bannerTable = Retrieve<BannerTable>(context, TableNames.BANNER);
        weddingTable = Retrieve<WeddingTable>(context, TableNames.WEDDING);
        rewardContentTable = Retrieve<RewardContentTable>(context, TableNames.REWARD_CONTENT);
        enchantScrollTable = Retrieve<EnchantScrollTable>(context, TableNames.ENCHANT_SCROLL);
        itemRemakeScrollTable = Retrieve<ItemRemakeScrollTable>(context, TableNames.ITEM_REMAKE_SCROLL);
        itemRepackingScrollTable = Retrieve<ItemRepackingScrollTable>(context, TableNames.ITEM_REPACKING_SCROLL);
        itemSocketScrollTable = Retrieve<ItemSocketScrollTable>(context, TableNames.ITEM_SOCKET_SCROLL);
        itemExchangeScrollTable = Retrieve<ItemExchangeScrollTable>(context, TableNames.ITEM_EXCHANGE_SCROLL);
        itemOptionConstantTable = Retrieve<ItemOptionConstantTable>(context, TableNames.ITEM_OPTION_CONSTANT);
        itemOptionRandomTable = Retrieve<ItemOptionRandomTable>(context, TableNames.ITEM_OPTION_RANDOM);
        itemOptionStaticTable = Retrieve<ItemOptionStaticTable>(context, TableNames.ITEM_OPTION_STATIC);
        itemOptionPickTable = Retrieve<ItemOptionPickTable>(context, TableNames.ITEM_OPTION_PICK);
        itemVariationTable = Retrieve<ItemVariationTable>(context, TableNames.ITEM_OPTION_VARIATION);
        accVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_ACC);
        armorVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_ARMOR);
        petVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_PET);
        weaponVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_WEAPON);
        dungeonRoomTable = Retrieve<DungeonRoomTable>(context, TableNames.DUNGEON_ROOM);
        dungeonRankRewardTable = Retrieve<DungeonRankRewardTable>(context, TableNames.DUNGEON_RANK_REWARD);
        dungeonConfigTable = Retrieve<DungeonConfigTable>(context, TableNames.DUNGEON_CONFIG);
        dungeonMissionTable = Retrieve<DungeonMissionTable>(context, TableNames.DUNGEON_MISSION);
        seasonDataTable = Retrieve<SeasonDataTable>(context, TableNames.SEASON_DATA);
        smartPushTable = Retrieve<SmartPushTable>(context, TableNames.SMART_PUSH);
        autoActionTable = Retrieve<AutoActionTable>(context, TableNames.AUTO_ACTION);

    }

    private static Lazy<T> Retrieve<T>(MetadataContext context, string key) where T : Table {
        var result = new Lazy<T>(() => {
            lock (context) {
                TableMetadata? row = context.TableMetadata.Find(key);
                if (row?.Table is not T result) {
                    throw new InvalidOperationException($"Row does not exist: {key}. Rerun Ingest!");
                }

                return result;
            }
        });

#if !DEBUG
        // No lazy loading for RELEASE build.
        _ = result.Value;
#endif
        return result;
    }
}
