﻿namespace KBot.Common
{
    using System;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class EpicStore
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("extensions")]
        public Extensions Extensions { get; set; }

        public Game[] Games => Data.Catalog.SearchStore.Games;
        public Game CurrentGame => Games[0];
        public Game UpcomingGame => Games[1];
    }

    public partial class Data
    {
        [JsonProperty("Catalog")]
        public Catalog Catalog { get; set; }
    }

    public partial class Catalog
    {
        [JsonProperty("searchStore")]
        public SearchStore SearchStore { get; set; }
    }

    public partial class SearchStore
    {
        [JsonProperty("elements")]
        public Game[] Games { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }

    public partial class Game
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("effectiveDate")]
        public DateTimeOffset EffectiveDate { get; set; }

        [JsonProperty("offerType")]
        public string OfferType { get; set; }

        [JsonProperty("expiryDate")]
        public object ExpiryDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isCodeRedemptionOnly")]
        public bool IsCodeRedemptionOnly { get; set; }

        [JsonProperty("keyImages")]
        public KeyImage[] KeyImages { get; set; }

        [JsonProperty("seller")]
        public Seller Seller { get; set; }

        [JsonProperty("productSlug")]
        public string ProductSlug { get; set; }

        [JsonProperty("urlSlug")]
        public string UrlSlug { get; set; }
        public string EpicUrl => "https://www.epicgames.com/store/en-US/p/" + UrlSlug;

        [JsonProperty("url")]
        public object Url { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("customAttributes")]
        public CustomAttribute[] CustomAttributes { get; set; }

        [JsonProperty("categories")]
        public Category[] Categories { get; set; }

        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }

        [JsonProperty("catalogNs")]
        public CatalogNs CatalogNs { get; set; }

        [JsonProperty("offerMappings")]
        public object[] OfferMappings { get; set; }

        [JsonProperty("price")]
        public Price Price { get; set; }

        [JsonProperty("promotions")]
        public Promotions Promotions { get; set; }
        
        public PromotionalOfferPromotionalOffer[] Discounts => Promotions.PromotionalOffers[0].PromotionalOffers;
    }

    public partial class CatalogNs
    {
        [JsonProperty("mappings")]
        public Mapping[] Mappings { get; set; }
    }

    public partial class Mapping
    {
        [JsonProperty("pageSlug")]
        public string PageSlug { get; set; }

        [JsonProperty("pageType")]
        public string PageType { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public partial class CustomAttribute
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }

    public partial class KeyImage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class Price
    {
        [JsonProperty("totalPrice")]
        public TotalPrice TotalPrice { get; set; }

        [JsonProperty("lineOffers")]
        public LineOffer[] LineOffers { get; set; }
    }

    public partial class LineOffer
    {
        [JsonProperty("appliedRules")]
        public AppliedRule[] AppliedRules { get; set; }
    }

    public partial class AppliedRule
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("discountSetting")]
        public AppliedRuleDiscountSetting DiscountSetting { get; set; }
    }

    public partial class AppliedRuleDiscountSetting
    {
        [JsonProperty("discountType")]
        public string DiscountType { get; set; }
    }

    public partial class TotalPrice
    {
        [JsonProperty("discountPrice")]
        public long DiscountPrice { get; set; }

        [JsonProperty("originalPrice")]
        public long OriginalPrice { get; set; }

        [JsonProperty("voucherDiscount")]
        public long VoucherDiscount { get; set; }

        [JsonProperty("discount")]
        public long Discount { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("currencyInfo")]
        public CurrencyInfo CurrencyInfo { get; set; }

        [JsonProperty("fmtPrice")]
        public FmtPrice FmtPrice { get; set; }
    }

    public partial class CurrencyInfo
    {
        [JsonProperty("decimals")]
        public long Decimals { get; set; }
    }

    public partial class FmtPrice
    {
        [JsonProperty("originalPrice")]
        public string OriginalPrice { get; set; }

        [JsonProperty("discountPrice")]
        public string DiscountPrice { get; set; }

        [JsonProperty("intermediatePrice")]
        public string IntermediatePrice { get; set; }
    }

    public partial class Promotions
    {
        [JsonProperty("promotionalOffers")]
        public PromotionalOffer[] PromotionalOffers { get; set; }

        [JsonProperty("upcomingPromotionalOffers")]
        public PromotionalOffer[] UpcomingPromotionalOffers { get; set; }
    }

    public partial class PromotionalOffer
    {
        [JsonProperty("promotionalOffers")]
        public PromotionalOfferPromotionalOffer[] PromotionalOffers { get; set; }
    }

    public partial class PromotionalOfferPromotionalOffer
    {
        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("discountSetting")]
        public PromotionalOfferDiscountSetting DiscountSetting { get; set; }
    }

    public partial class PromotionalOfferDiscountSetting
    {
        [JsonProperty("discountType")]
        public string DiscountType { get; set; }

        [JsonProperty("discountPercentage")]
        public long DiscountPercentage { get; set; }
    }

    public partial class Seller
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Tag
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }

    public partial class Paging
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public partial class Extensions
    {
    }

    public partial class EpicStore
    {
        public static EpicStore FromJson(string json) => JsonConvert.DeserializeObject<EpicStore>(json, KBot.Common.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}



/*using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KBot.Common;

public class StoreModel
{
    [JsonProperty("data")] private Data Data { get; }

    public List<Game> Games => Data.Catalog.Search.Games;

    public Game CurrentGame => Games[0];
    public Game UpcomingGame => Games[1];
    //[JsonProperty("extensions")] public Extensions Extensions { get; set; }
}

public class Data
{
    [JsonProperty("catalog")] public Catalog Catalog { get; set; }
}

public class Catalog
{
    [JsonProperty("searchStore")] public SearchStore Search { get; set; }
}

public class SearchStore
{
    [JsonProperty("elements")] public List<Game> Games { get; set; }
}

public class Game
{
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("description")] public string Description { get; set; }
    //[JsonProperty("offerType")] public OfferType OfferType { get; set; }
    //[JsonProperty("status")] public Status Status { get; set; }
    [JsonProperty("keyImages")] public List<KeyImage> Images { get; set; }
    [JsonProperty("seller")] private Seller Seller { get; }
    public string SellerName => Seller.Name;
    [JsonProperty("urlSlug")] public string UrlSlug { get; set; }
    public string Url => "https://www.epicgames.com/store/en-US/p/" + UrlSlug;
    
    [JsonProperty("customAttributes")] public List<CustomAttribute> CustomAttributes { get; set; }
    [JsonProperty("price")] private Price _Price { get; }
    public TotalPrice Price => _Price.TotalPrice;
    [JsonProperty("promotions")] private Promotions Promotions { get; }
    public List<PromotionalOffer> Discounts => Promotions.PromotionalOffers[0].Offers;
}

public class Promotions
{
    [JsonProperty("promotionalOffers")] public List<PromotionalOffers> PromotionalOffers { get; set; }
}

public class PromotionalOffers
{
    [JsonProperty("promotionalOffers")] public List<PromotionalOffer> Offers { get; set; }
}

public class PromotionalOffer
{
    [JsonProperty("startDate")] public DateTime StartDate { get; set; }
    [JsonProperty("endDate")] public DateTime EndDate { get; set; }
    [JsonProperty("discountSettings")] public DiscountSettings DiscountSettings { get; set; }
}

public class DiscountSettings
{
    [JsonProperty("discountType")] public string DiscountType { get; set; }
    [JsonProperty("discountPercentage")] public int DiscountPercentage { get; set; }
}

public class Price
{
    [JsonProperty("totalprice")] public TotalPrice TotalPrice { get; set; }
}

public class TotalPrice
{
    [JsonProperty("discountPrice")] public int DiscountPrice { get; set; }
    [JsonProperty("originalPrice")] public int OriginalPrice { get; set; }
    [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
    [JsonProperty("fmtPrice")] public FmtPrice CountryPrice { get; set; }
}

public class FmtPrice
{
    [JsonProperty("originalPrice")] public string OriginalPrice { get; set; }
    [JsonProperty("discountPrice")] public string DiscountPrice { get; set; }
}

public class CustomAttribute
{
}

public class Seller
{
    [JsonProperty("name")] public string Name { get; set; }
}

public class KeyImage
{
    //[JsonProperty("type")] public ImageType Type { get; set; }
    [JsonProperty("url")] public string Url { get; set; }
}*/