using System.Net.Http.Json;
using System.Web;
using eShop.WebAppComponents.Catalog;

namespace eShop.WebAppComponents.Services;

public class CatalogService(HttpClient httpClient) : ICatalogService
{
    private readonly string remoteServiceBaseUrl = "api/catalog/";

    public async Task<CatalogItem?> GetCatalogItem(int id)
    {
        try
        {
            var uri = $"{remoteServiceBaseUrl}items/{id}";
            return await httpClient.GetFromJsonAsync<CatalogItem>(uri);
        }
        catch
        {
            return null;
        }
    }

    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type)
    {
        try
        {
            var uri = GetAllCatalogItemsUri(remoteServiceBaseUrl, pageIndex, pageSize, brand, type);
            var result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);
            return result ?? new CatalogResult(pageIndex, pageSize, 0, []);
        }
        catch
        {
            return new CatalogResult(pageIndex, pageSize, 0, []);
        }
    }

    public async Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids)
    {
        try
        {
            var uri = $"{remoteServiceBaseUrl}items/by?ids={string.Join("&ids=", ids)}";
            var result = await httpClient.GetFromJsonAsync<List<CatalogItem>>(uri);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CatalogResult> GetCatalogItemsWithSemanticRelevance(int page, int take, string text)
    {
        try
        {
            var url = $"{remoteServiceBaseUrl}items/withsemanticrelevance?text={HttpUtility.UrlEncode(text)}&pageIndex={page}&pageSize={take}";
            var result = await httpClient.GetFromJsonAsync<CatalogResult>(url);
            return result ?? new CatalogResult(page, take, 0, []);
        }
        catch
        {
            return new CatalogResult(page, take, 0, []);
        }
    }

    public async Task<IEnumerable<CatalogBrand>> GetBrands()
    {
        try
        {
            var uri = $"{remoteServiceBaseUrl}catalogBrands";
            var result = await httpClient.GetFromJsonAsync<CatalogBrand[]>(uri);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IEnumerable<CatalogItemType>> GetTypes()
    {
        try
        {
            var uri = $"{remoteServiceBaseUrl}catalogTypes";
            var result = await httpClient.GetFromJsonAsync<CatalogItemType[]>(uri);
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type)
    {
        string filterQs = string.Empty;

        if (type.HasValue)
        {
            filterQs += $"type={type.Value}&";
        }
        if (brand.HasValue)
        {
            filterQs += $"brand={brand.Value}&";
        }

        return $"{baseUri}items?{filterQs}pageIndex={pageIndex}&pageSize={pageSize}";
    }
}
