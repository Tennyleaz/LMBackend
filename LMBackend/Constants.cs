namespace LMBackend
{
    internal static class Constants
    {
        public const string LLM_BASE_URL = "http://192.168.41.133";
        public const string LLM_ENDPOINT = LLM_BASE_URL + ":9090/v1";
        public const string METRICS_ENDPOINT = LLM_BASE_URL + ":9090/metrics";
        public const string DOCKER_ENDPOINT = LLM_BASE_URL + ":2375";
        public const string DOCKER_NAME = "vllm_tenny";
        public const string CHROMA_ENDPOINT = LLM_BASE_URL + ":9092";
        public const string EMBEDDING_ENDPOINT = "http://10.102.196.42:18307/v1/embeddings";
        public const string LLM_KEY = "tenny";
        public const string DEFAULT_MODEL = "meta-llama/Llama-3.2-3B-Instruct";
        public const int MAX_TOKEN = 4096;
        public const string PGSQL_IP = "192.168.41.133";
        public const string PGSQL_PORT = "9091";
        public const string SCRAP_ENDPOINT = LLM_BASE_URL + ":9094/scrape";
        public const string WHISPER_MODEL_PATH = "";  // ggml model path
        public const string WHISPER_BIN_PATH = "";  // whisper.cpp binary
        public const int WHISPER_CHUNK_SECONDS = 5;
        public const int WHISPER_SAMPLE_RATE = 16000;
        public const string TTS_ENDPOINT = LLM_BASE_URL + ":9095/tts";
    }
}
