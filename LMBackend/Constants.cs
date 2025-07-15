namespace LMBackend
{
    internal static class Constants
    {
        public const string LLM_BASE_URL = "http://192.168.41.133";
        public const string LLM_ENDPOINT = LLM_BASE_URL + ":9090/v1";
        public const string METRICS_ENDPOINT = LLM_BASE_URL + ":9090/metrics";
        public const string DOCKER_ENDPOINT = LLM_BASE_URL + ":2375";
        public const string DOCKER_NAME = "vllm_tenny";
        public const string LLM_KEY = "tenny";
        public const string DEFAULT_MODEL = "meta-llama/Llama-3.2-3B-Instruct";
        public const int MAX_TOKEN = 4096;
        public const string PGSQL_IP = "192.168.41.133";
    }
}
