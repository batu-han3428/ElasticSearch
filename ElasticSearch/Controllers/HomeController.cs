using ElasticSearch.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Diagnostics;

namespace ElasticSearch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConnectionSettings connSettings;

        private readonly ElasticClient elasticClient;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            connSettings = new ConnectionSettings(new Uri("http://localhost:9200/")).BasicAuthentication("elastic", "t82KZDcP8tDtQsSfRG1R")
           .DefaultIndex("urunler").DefaultMappingFor<Urun>(m => m.IndexName("urunler"));
            elasticClient = new ElasticClient(connSettings);
        }

        public IActionResult Index()
        {
            try
            {
                Indexing();
                return View(UrunBul());
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }
        private void Indexing()
        {
            List<Urun> urunler = new List<Urun>();
            Urun urun1 = new Urun()
            {
                Id = "1",
                Ad = "Samsung A50 Yeni",
                Aciklama = "Samsung A50 yeni model",
                Fiyat = "10.000TL",
                Fotograf = "https://cdn.shopify.com/s/files/1/0289/5790/0859/products/samsung-a50-screen-repair-funtech-repair-sama50lcd.jpg?v=1591806465"
            };
            Urun urun2 = new Urun()
            {
                Id = "2",
                Ad = "Iphone 13",
                Aciklama = "Iphone 13 yeni model",
                Fiyat = "30.000TL",
                Fotograf = "https://store.storeimages.cdn-apple.com/4668/as-images.apple.com/is/iphone-13-finish-select-202207-6-1inch-blue?wid=2560&hei=1440&fmt=jpeg&qlt=95&.v=1656712888128"
            };
            Urun urun3 = new Urun()
            {
                Id = "3",
                Ad = "Huawei Nova",
                Aciklama = "Huawei Nova 10 yeni model",
                Fiyat = "20.000TL",
                Fotograf = "https://img01.huaweifile.com/eu/tr/huawei/uomcdn/TRHW/pms/202209/gbom/6941487272891/428_428_6FF944BCA74385C3D7FE356E84A0FC22mp.png"
            };
            Urun urun4 = new Urun()
            {
                Id = "4",
                Ad = "Samsung A52",
                Aciklama = "Samsung A52 yeni model",
                Fiyat = "15.000TL",
                Fotograf = "https://cdn.shopify.com/s/files/1/0289/5790/0859/products/samsung-a50-screen-repair-funtech-repair-sama50lcd.jpg?v=1591806465"
            };
            urunler.Add(urun1);
            urunler.Add(urun2);
            urunler.Add(urun3);
            urunler.Add(urun4);

            var defaultIndex = "urunler";
            var client = new ElasticClient();

            if (elasticClient.Indices.Exists(defaultIndex).Exists)
            {
                client.Indices.Delete(defaultIndex);
            }

            if (!elasticClient.Indices.Exists("urun_alias").Exists)
            {
                client.Indices.Create(defaultIndex, c => c
                    .Mappings(m => m
                        .Map<Urun>(mm => mm
                            .AutoMap()
                        )
                    ).Aliases(a => a.Alias("urun_alias"))
                );
            }

            // Bulk Insert
            var bulkIndexer = new BulkDescriptor();

            foreach (var document in urunler)
            {
                bulkIndexer.Index<Urun>(i => i
                    .Document(document)
                    .Id(document.Id)
                    .Index("urunler"));
            }

            elasticClient.Bulk(bulkIndexer);

        }
        public List<Urun> UrunBul(string value = "")
        {
            try
            {
                var urunler = elasticClient.Search<Urun>(s => s
    .Query(x=>x.Bool(x=>x.Filter(x=>x.Bool(x=>x.Should(x=>x.QueryString(x=>x.DefaultField("ad").Query("*"+value+"*").DefaultOperator(Operator.And)))))))
).Documents.ToList();
                return urunler;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}