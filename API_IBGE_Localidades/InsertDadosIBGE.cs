using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace API_IBGE_Localidades
{
    public class Estado
    {
        public int id { get; set; }
        public string sigla { get; set; }
        public string nome { get; set; }
        public Regiao regiao { get; set; }
    }

    public class Regiao
    {
        public int id { get; set; }
        public string sigla { get; set; }
        public string nome { get; set; }
    }

    public class Municipio
    {
        public int id { get; set; }
        public string nome { get; set; }
    }

    class InsertDadosIBGE
    {
        static async Task Main(string[] args)
        {
            string apiUrlEstados = "https://servicodados.ibge.gov.br/api/v1/localidades/estados";

            string connectionString = "Server=ITLNB064\\SQLEXPRESS;Database=dbIBGE;Trusted_Connection=True;TrustServerCertificate=True;";

            List<Estado> estados = await ObterEstadosDoIBGE(apiUrlEstados);

            InserirEstadosNoBanco(estados, connectionString);

            Console.WriteLine("Estados inseridos com sucesso!");

            foreach (var estado in estados)
            {
                string apiUrlMunicipios = $"https://servicodados.ibge.gov.br/api/v1/localidades/estados/{estado.sigla}/municipios";

                List<Municipio> municipios = await ObterMunicipiosPorEstado(apiUrlMunicipios);

                InserirMunicipiosNoBanco(municipios, connectionString, estado.id);
                Console.WriteLine($"Municípios do estado {estado.nome} inseridos com sucesso!");
            }

            Console.WriteLine("Processo concluído!");
        }

        public static async Task<List<Estado>> ObterEstadosDoIBGE(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                List<Estado> estados = JsonSerializer.Deserialize<List<Estado>>(jsonResponse);

                return estados;
            }
        }

        public static async Task<List<Municipio>> ObterMunicipiosPorEstado(string apiUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                List<Municipio> municipios = JsonSerializer.Deserialize<List<Municipio>>(jsonResponse);

                return municipios;
            }
        }

        public static void InserirEstadosNoBanco(List<Estado> estados, string connectionString)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var estado in estados)
                {
                    string query = "INSERT INTO Estados (Id, Sigla, Nome, Regiao) VALUES (@Id, @Sigla, @Nome, @Regiao)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", estado.id);
                        cmd.Parameters.AddWithValue("@Sigla", estado.sigla);
                        cmd.Parameters.AddWithValue("@Nome", estado.nome);
                        cmd.Parameters.AddWithValue("@Regiao", estado.regiao.nome);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void InserirMunicipiosNoBanco(List<Municipio> municipios, string connectionString, int estadoId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var municipio in municipios)
                {
                    string query = "INSERT INTO Municipios (Id, Nome, EstadoId) VALUES (@Id, @Nome, @EstadoId)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", municipio.id);
                        cmd.Parameters.AddWithValue("@Nome", municipio.nome);
                        cmd.Parameters.AddWithValue("@EstadoId", estadoId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
