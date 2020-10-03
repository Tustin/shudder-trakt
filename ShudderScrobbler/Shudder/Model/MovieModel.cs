using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShudderScrobbler.Shudder.Model
{
    public partial class MovieModel
    {
        [JsonProperty("video")]
        public Video Video { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("hreflang")]
        public object Hreflang { get; set; }

        [JsonProperty("media")]
        public object Media { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("deprecation")]
        public object Deprecation { get; set; }
    }

    public partial class Video
    {
        [JsonProperty("bcovId")]
        public string BcovId { get; set; }

        [JsonProperty("bcovStatus")]
        public string BcovStatus { get; set; }

        [JsonProperty("bcovAccountId")]
        public string BcovAccountId { get; set; }

        [JsonProperty("licensestartdate")]
        public long Licensestartdate { get; set; }

        [JsonProperty("licenseenddate")]
        public long Licenseenddate { get; set; }

        [JsonProperty("publishdate")]
        public long Publishdate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("tms_id")]
        public string TmsId { get; set; }

        [JsonProperty("photoVideoMetadataIPTC")]
        public PhotoVideoMetadataIptc PhotoVideoMetadataIptc { get; set; }

        [JsonProperty("amcMetadata_movie")]
        public AmcMetadataMovie AmcMetadataMovie { get; set; }

        [JsonProperty("_Id")]
        public string Id { get; set; }
    }

    public partial class AmcMetadataMovie
    {
        [JsonProperty("sourceid")]
        public string Sourceid { get; set; }

        [JsonProperty("relatedContent")]
        public object[] RelatedContent { get; set; }

        [JsonProperty("isOriginal")]
        public bool IsOriginal { get; set; }

        [JsonProperty("introStarts")]
        public string IntroStarts { get; set; }

        [JsonProperty("short_description")]
        public ShortDescription[] ShortDescription { get; set; }

        [JsonProperty("trailers")]
        public string[] Trailers { get; set; }
    }

    public partial class ShortDescription
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }
    }

    public partial class PhotoVideoMetadataIptc
    {
        [JsonProperty("dateReleased")]
        public long DateReleased { get; set; }

        [JsonProperty("ratings")]
        public Rating[] Ratings { get; set; }

        [JsonProperty("description")]
        public ShortDescription[] Description { get; set; }

        [JsonProperty("genres")]
        public string[] Genres { get; set; }

        [JsonProperty("headline")]
        public ShortDescription[] Headline { get; set; }

        [JsonProperty("keywords")]
        public object[] Keywords { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("locationsCreated")]
        public string[] LocationsCreated { get; set; }

        [JsonProperty("title")]
        public ShortDescription[] Title { get; set; }

        [JsonProperty("contributors")]
        public Contributor[] Contributors { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("snapShotLinks")]
        public SnapShotLink[] SnapShotLinks { get; set; }
    }

    public partial class Contributor
    {
        [JsonProperty("billingOrder")]
        public string BillingOrder { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Rating
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public partial class SnapShotLink
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }
    }
}
