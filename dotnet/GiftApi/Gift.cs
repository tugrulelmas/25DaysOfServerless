using System;
using Newtonsoft.Json;

namespace Abioka.GiftApi {
    public class Gift {
        [JsonProperty ("id")]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}