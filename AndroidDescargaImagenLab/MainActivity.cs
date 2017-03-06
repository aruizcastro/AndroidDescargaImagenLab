using Android.App;
using Android.Widget;
using Android.OS;
using XamarinDiplomado.Participants.Startup;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.WindowsAzure.Storage.Table;

namespace AndroidDescargaImagenLab
{
    [Activity(Label = "AndroidDescargaImagenLab", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback
    {
        ImageView Imagen;
        GoogleMap googleMap;
        MapView mapView;
        double latitud, longitud;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            Startup startup = new Startup("Aland Ruiz Castro","ar1987@hotmail.com",1,1);
            startup.Init();

            Button btnImagen = FindViewById<Button>
                (Resource.Id.btnBajar);
            Imagen = FindViewById<ImageView>(Resource.Id.imagen);
            btnImagen.Click += async delegate
            {
                try
                {
                    string carpeta = System.Environment.GetFolderPath
                        (System.Environment.SpecialFolder.Personal);
                    string archivoLocal = "mifoto1.jpg";
                    string ruta = System.IO.Path.Combine(carpeta, archivoLocal);
                    CloudStorageAccount cuentaAlmacenamiento = CloudStorageAccount.Parse
                        ("DefaultEndpointsProtocol=https;AccountName=subirimagen;AccountKey=hB623e/swtKMq/3EA++gCp4UAmGnyHBVJYidnrgo+y5sz1IBb4wExZwYT5JmrYNAq3HdQmBiRj10bm/29dIY0A==");
                    CloudBlobClient clienteBlob = cuentaAlmacenamiento.CreateCloudBlobClient();
                    CloudBlobContainer contenedor = clienteBlob.GetContainerReference("laboratorio1");
                    CloudBlockBlob recursoBlob = contenedor.GetBlockBlobReference("mifoto1.jpg");
                    var stream = File.OpenWrite(ruta);
                    await recursoBlob.DownloadToStreamAsync(stream);
                    Android.Net.Uri rutaImagen = Android.Net.Uri.Parse(ruta);
                    Imagen.SetImageURI(rutaImagen);
                    CloudTableClient tableClient = cuentaAlmacenamiento.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference("Ubicaciones");
                    TableOperation retrieveOperation = TableOperation.Retrieve<UbicacionEntity>("mifoto1.jpg", "Colombia");
                    TableResult retrievedResult = await table.ExecuteAsync(retrieveOperation);
                    if (retrievedResult.Result != null)
                        longitud = ((UbicacionEntity)retrievedResult.Result).Longitud;
                    latitud = ((UbicacionEntity)retrievedResult.Result).Latitud;
                    mapView = FindViewById<MapView>(Resource.Id.map);
                    mapView.OnCreate(bundle);
                    mapView.GetMapAsync(this);
                    MapsInitializer.Initialize(this);
                }
                catch (StorageException ex)
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                }
            };

        }

        public void OnMapReady(GoogleMap googleMap)
        {
            this.googleMap = googleMap;
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(new LatLng(latitud, longitud));
            builder.Zoom(17);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            this.googleMap.AnimateCamera(cameraUpdate);
        }
    }
    public class UbicacionEntity : TableEntity
    {
        public UbicacionEntity(string Archivo, string Pais)
        {
            this.PartitionKey = Archivo;
            this.RowKey = Pais;
        }
        public UbicacionEntity() { }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}

