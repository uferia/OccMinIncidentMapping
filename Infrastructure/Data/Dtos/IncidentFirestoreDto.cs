using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Dtos
{
    [FirestoreData]
    public class IncidentFirestoreDto
    {
        [FirestoreProperty]
        public double Latitude { get; set; }

        [FirestoreProperty]
        public double Longitude { get; set; }

        [FirestoreProperty]
        public string Hazard { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
    }
}
