using DoctorWebApi.Helpers;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text.Json;

namespace DoctorWebApi.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase };    
            response.Headers.Add("Pagination", System.Text.Json.JsonSerializer.Serialize(header, jsonOptions));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
