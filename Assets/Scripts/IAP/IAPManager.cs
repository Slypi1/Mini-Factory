using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour
{
    private const string ProductId = "coins_pack_small";
    private const int CoinsReward = 100;

     [SerializeField] private Bootstrapper _bootstrapper;
     [SerializeField] private Button _buyCoinsButton;

    private StoreController storeController;
    private bool isReady;

    private async void Start()
    {
        _buyCoinsButton.onClick.AddListener(BuySmallCoinsPack);
        
        if (_bootstrapper == null)
        {
            Debug.LogError("IAPManager: Bootstrapper is not assigned.");
            return;
        }

        try
        {
            storeController = UnityIAPServices.StoreController();

            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseFailed += OnPurchaseFailed;

            await storeController.Connect();

            var products = new List<ProductDefinition>
            {
                new ProductDefinition(ProductId, ProductType.Consumable)
            };

            storeController.FetchProducts(products);
            _bootstrapper.TrackEvent("iap_initialization_succeeded");
        }
        catch (Exception exception)
        {
            _bootstrapper.TrackEvent(
                "iap_initialization_failed",
                new Dictionary<string, object>
                {
                    { "reason", exception.Message }
                });
            Debug.LogError($"[IAP] Initialization failed: {exception.Message}");
            _bootstrapper.TrackEvent("iap_initialization_failed");
            
        }
    }

    public void BuySmallCoinsPack()
    {
        if (!isReady || storeController == null)
        {
            Debug.LogWarning("[IAP] Store is not ready.");
            _bootstrapper.TrackEvent("iap_unavailable");
            return;
        }

        Product product = null;

        foreach (Product item in storeController.GetProducts())
        {
            if (item.definition.id == ProductId)
            {
                product = item;
                break;
            }
        }

        if (product == null)
        {
            Debug.LogWarning($"[IAP] Product unavailable: {ProductId}");
            _bootstrapper.TrackEvent("iap_unavailable");
            return;
        }

        storeController.PurchaseProduct(product);
    }

    private void OnProductsFetched(List<Product> products)
    {
        isReady = products.Exists(
            product => product.definition.id == ProductId);

        if (!isReady)
        {
            Debug.LogWarning($"[IAP] {ProductId} was not found.");
            _bootstrapper.TrackEvent("iap_unavailable");
            return;
        }

        storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        isReady = false;
        Debug.LogWarning($"[IAP] Product fetch failed: {failure}");
        _bootstrapper.TrackEvent("iap_products_fetch_failed");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        _bootstrapper.TrackEvent("iap_purchases_fetched");
    }

    private void OnPurchasesFetchFailed(
        PurchasesFetchFailureDescription failure)
    {
        Debug.LogWarning($"[IAP] Purchase history fetch failed: {failure}");
        _bootstrapper.TrackEvent("iap_purchases_fetch_failed");
    }

    private void OnStoreDisconnected(
        StoreConnectionFailureDescription failure)
    {
        isReady = false;
        Debug.LogWarning($"[IAP] Store disconnected: {failure}");
        _bootstrapper.TrackEvent("iap_initialization_failed");
    }

    private void OnPurchasePending(PendingOrder order)
    {
        bool rewardGranted = false;

        foreach (var item in order.CartOrdered.Items())
        {
            if (item.Product.definition.id != ProductId)
                continue;

            _bootstrapper.GrantCurrency(CoinsReward);
            rewardGranted = true;

            _bootstrapper.TrackEvent(
                "purchase_succeeded",
                new Dictionary<string, object>
                {
                    { "product_id", ProductId },
                    { "coins", CoinsReward }
                });
        }

        if (rewardGranted)
        {
            storeController.ConfirmPurchase(order);
        }
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {order}");
        _bootstrapper.TrackEvent("purchase_failed");
    }

    private void OnDestroy()
    {
        _buyCoinsButton.onClick.RemoveListener(BuySmallCoinsPack);
        
        if (storeController == null)
            return;

        storeController.OnStoreDisconnected -= OnStoreDisconnected;
        storeController.OnProductsFetched -= OnProductsFetched;
        storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
        storeController.OnPurchasesFetched -= OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        storeController.OnPurchasePending -= OnPurchasePending;
        storeController.OnPurchaseFailed -= OnPurchaseFailed;
    }
}
