using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance;

    public const string ProductNoAds = "noads";
    public const string ProductHintPack = "hintpack";
    public const string ProductVIP = "vip";
    public const string ProductSupport = "support";
    public const string ProductLivesPack = "livespack";

    private const string NoAdsKey = "NoAdsPurchased";
    private const string VIPKey = "VIPPurchased";
    private const string HintKey = "HintCount";

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    // ──────────────────────────────────────────
    // Başlangıç
    // ──────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePurchasing();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(ProductNoAds, ProductType.NonConsumable);
        builder.AddProduct(ProductVIP, ProductType.NonConsumable);
        builder.AddProduct(ProductSupport, ProductType.NonConsumable);
        builder.AddProduct(ProductHintPack, ProductType.Consumable);
        builder.AddProduct(ProductLivesPack, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    // ──────────────────────────────────────────
    // Satın Alma
    // ──────────────────────────────────────────

    public void BuyNoAds() => BuyProduct(ProductNoAds);
    public void BuyHintPack() => BuyProduct(ProductHintPack);
    public void BuyVIP() => BuyProduct(ProductVIP);
    public void BuySupport() => BuyProduct(ProductSupport);
    public void BuyLivesPack() => BuyProduct(ProductLivesPack);

    private void BuyProduct(string productId)
    {
        if (storeController == null)
        {
            Debug.LogWarning("Store henüz hazır değil.");
            return;
        }

        Product product = storeController.products.WithID(productId);

        if (product != null && product.availableToPurchase)
            storeController.InitiatePurchase(product);
        else
            Debug.LogWarning("Ürün satın alınamıyor: " + productId);
    }

    // ──────────────────────────────────────────
    // Durum Kontrol
    // ──────────────────────────────────────────

    public bool IsNoAdsPurchased() => PlayerPrefs.GetInt(NoAdsKey, 0) == 1;
    public bool IsVIPPurchased() => PlayerPrefs.GetInt(VIPKey, 0) == 1;
    public int GetHintCount() => PlayerPrefs.GetInt(HintKey, 0);

    public void UseHint()
    {
        int count = GetHintCount();
        if (count > 0)
        {
            PlayerPrefs.SetInt(HintKey, count - 1);
            PlayerPrefs.Save();
        }
    }

    // ──────────────────────────────────────────
    // Satın Alma Geri Yükleme
    // ──────────────────────────────────────────

    public void RestorePurchases()
    {
#if UNITY_IOS
        var apple = extensionProvider.GetExtension<IAppleExtensions>();
        apple.RestoreTransactions((result, error) =>
        {
            Debug.Log("Satın almalar geri yüklendi: " + result);
        });
#endif
    }

    // ──────────────────────────────────────────
    // IStoreListener Callbacks
    // ──────────────────────────────────────────

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("IAP başarıyla başlatıldı.");

        // Ürünleri listele
        foreach (var product in controller.products.all)
            Debug.Log($"Ürün: {product.definition.id} | Mevcut: {product.availableToPurchase}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning("IAP başlatma hatası: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning("IAP başlatma hatası: " + error + " | " + message);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;

        if (productId == ProductNoAds)
        {
            PlayerPrefs.SetInt(NoAdsKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Reklamsız deneyim aktif edildi!");
        }
        else if (productId == ProductVIP)
        {
            PlayerPrefs.SetInt(NoAdsKey, 1);
            PlayerPrefs.SetInt(VIPKey, 1);
            PlayerPrefs.Save();
            Debug.Log("VIP aktif edildi!");
        }
        else if (productId == ProductHintPack)
        {
            int current = GetHintCount();
            PlayerPrefs.SetInt(HintKey, current + 10);
            PlayerPrefs.Save();
            Debug.Log("10 ipucu eklendi!");
        }
        else if (productId == ProductLivesPack)
        {
            if (LivesManager.Instance != null)
                LivesManager.Instance.AddPurchasedLives(5);

            Debug.Log("5 can eklendi!");

            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.HideAllPanels();

            if (ShopManager.Instance != null)
                ShopManager.Instance.CloseShop();

            ChapterManager.ReloadCurrentScene();
        }
        else if (productId == ProductSupport)
        {
            Debug.Log("Geliştirici desteklendi, teşekkürler!");
        }

        // Firebase'e satın alma logunu kaydet
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsReady())
            FirebaseManager.Instance.LogPurchase(productId);

        // Firebase'e oyuncu verilerini güncelle
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsReady())
            FirebaseManager.Instance.SavePlayerData();

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning("Satın alma başarısız: " + product.definition.id + " | " + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning("Satın alma başarısız: " + product.definition.id + " | " + failureDescription.message);
    }
}