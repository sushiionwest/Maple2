using Maple2.Database.Context;
using Maple2.Model.Common;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class ServerTableMetadataStorage {
    private Lazy<InstanceFieldTable> instanceFieldTable = null!;
    private Lazy<ScriptConditionTable> scriptConditionTable = null!;
    private Lazy<ScriptFunctionTable> scriptFunctionTable = null!;
    private Lazy<ScriptEventConditionTable> scriptEventConditionTable = null!;
    private Lazy<JobConditionTable> jobConditionTable = null!;
    private Lazy<BonusGameTable> bonusGameTable = null!;
    private Lazy<GlobalDropItemBoxTable> globalDropItemBoxTable = null!;
    private Lazy<UserStatTable> userStatTable = null!;
    private Lazy<IndividualDropItemTable> individualDropItemTable = null!;
    private Lazy<PrestigeExpTable> prestigeExpTable = null!;
    private Lazy<PrestigeIdExpTable> prestigeIdExpTable = null!;
    private Lazy<TimeEventTable> timeEventTable = null!;
    private Lazy<GameEventTable> gameEventTable = null!;
    private Lazy<OxQuizTable> oxQuizTable = null!;
    private Lazy<ItemMergeTable> itemMergeTable = null!;
    private Lazy<ShopTable> shopTable = null!;
    private Lazy<ShopItemTable> shopItemTable = null!;
    private Lazy<BeautyShopTable> beautyShopTable = null!;
    private Lazy<MeretMarketTable> meretMarketTable = null!;
    private Lazy<FishTable> fishTable = null!;
    private Lazy<CombineSpawnTable> combineSpawnTable = null!;
    private Lazy<EnchantOptionTable> enchantOptionTable = null!;
    private Lazy<UnlimitedEnchantOptionTable> unlimitedEnchantOptionTable = null!;
    private Lazy<ConstantsTable> constantsTable = null!;

    public InstanceFieldTable InstanceFieldTable => instanceFieldTable.Value;
    public ScriptConditionTable ScriptConditionTable => scriptConditionTable.Value;
    public ScriptFunctionTable ScriptFunctionTable => scriptFunctionTable.Value;
    public ScriptEventConditionTable ScriptEventConditionTable => scriptEventConditionTable.Value;
    public JobConditionTable JobConditionTable => jobConditionTable.Value;
    public BonusGameTable BonusGameTable => bonusGameTable.Value;
    public GlobalDropItemBoxTable GlobalDropItemBoxTable => globalDropItemBoxTable.Value;
    public UserStatTable UserStatTable => userStatTable.Value;
    public IndividualDropItemTable IndividualDropItemTable => individualDropItemTable.Value;
    public PrestigeExpTable PrestigeExpTable => prestigeExpTable.Value;
    public PrestigeIdExpTable PrestigeIdExpTable => prestigeIdExpTable.Value;
    public TimeEventTable TimeEventTable => timeEventTable.Value;
    public GameEventTable GameEventTable => gameEventTable.Value;
    public OxQuizTable OxQuizTable => oxQuizTable.Value;
    public ItemMergeTable ItemMergeTable => itemMergeTable.Value;
    public ShopTable ShopTable => shopTable.Value;
    public ShopItemTable ShopItemTable => shopItemTable.Value;
    public BeautyShopTable BeautyShopTable => beautyShopTable.Value;
    public MeretMarketTable MeretMarketTable => meretMarketTable.Value;
    public FishTable FishTable => fishTable.Value;
    public CombineSpawnTable CombineSpawnTable => combineSpawnTable.Value;
    public EnchantOptionTable EnchantOptionTable => enchantOptionTable.Value;
    public UnlimitedEnchantOptionTable UnlimitedEnchantOptionTable => unlimitedEnchantOptionTable.Value;
    public ConstantsTable ConstantsTable => constantsTable.Value;

    private readonly MetadataContext context;

    public ServerTableMetadataStorage(MetadataContext context) {
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
        instanceFieldTable = Retrieve<InstanceFieldTable>(context, ServerTableNames.INSTANCE_FIELD);
        scriptConditionTable = Retrieve<ScriptConditionTable>(context, ServerTableNames.SCRIPT_CONDITION);
        scriptFunctionTable = Retrieve<ScriptFunctionTable>(context, ServerTableNames.SCRIPT_FUNCTION);
        scriptEventConditionTable = Retrieve<ScriptEventConditionTable>(context, ServerTableNames.SCRIPT_EVENT);
        jobConditionTable = Retrieve<JobConditionTable>(context, ServerTableNames.JOB_CONDITION);
        bonusGameTable = Retrieve<BonusGameTable>(context, ServerTableNames.BONUS_GAME);
        globalDropItemBoxTable = Retrieve<GlobalDropItemBoxTable>(context, ServerTableNames.GLOBAL_DROP_ITEM_BOX);
        userStatTable = Retrieve<UserStatTable>(context, ServerTableNames.USER_STAT);
        individualDropItemTable = Retrieve<IndividualDropItemTable>(context, ServerTableNames.INDIVIDUAL_DROP_ITEM);
        prestigeExpTable = Retrieve<PrestigeExpTable>(context, ServerTableNames.PRESTIGE_EXP);
        prestigeIdExpTable = Retrieve<PrestigeIdExpTable>(context, ServerTableNames.PRESTIGE_ID_EXP);
        timeEventTable = Retrieve<TimeEventTable>(context, ServerTableNames.TIME_EVENT);
        gameEventTable = Retrieve<GameEventTable>(context, ServerTableNames.GAME_EVENT);
        oxQuizTable = Retrieve<OxQuizTable>(context, ServerTableNames.OX_QUIZ);
        itemMergeTable = Retrieve<ItemMergeTable>(context, ServerTableNames.ITEM_MERGE);
        shopTable = Retrieve<ShopTable>(context, ServerTableNames.SHOP);
        shopItemTable = Retrieve<ShopItemTable>(context, ServerTableNames.SHOP_ITEM);
        beautyShopTable = Retrieve<BeautyShopTable>(context, ServerTableNames.BEAUTY_SHOP);
        meretMarketTable = Retrieve<MeretMarketTable>(context, ServerTableNames.MERET_MARKET);
        fishTable = Retrieve<FishTable>(context, ServerTableNames.FISH);
        combineSpawnTable = Retrieve<CombineSpawnTable>(context, ServerTableNames.COMBINE_SPAWN);
        enchantOptionTable = Retrieve<EnchantOptionTable>(context, ServerTableNames.ENCHANT_OPTION);
        unlimitedEnchantOptionTable = Retrieve<UnlimitedEnchantOptionTable>(context, ServerTableNames.UNLIMITED_ENCHANT_OPTION);
        constantsTable = Retrieve<ConstantsTable>(context, ServerTableNames.CONSTANTS);
    }

    public IEnumerable<GameEvent> GetGameEvents() {
        foreach ((int id, GameEventMetadata gameEvent) in GameEventTable.Entries) {
            if (gameEvent.EndTime < DateTime.Now) {
                continue;
            }

            yield return new GameEvent(gameEvent);
        }
    }

    private static Lazy<T> Retrieve<T>(MetadataContext context, string key) where T : ServerTable {
        var result = new Lazy<T>(() => {
            lock (context) {
                ServerTableMetadata? row = context.ServerTableMetadata.Find(key);
                if (row?.Table is not T result) {
                    throw new InvalidOperationException($"Row does not exist: {key}");
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
