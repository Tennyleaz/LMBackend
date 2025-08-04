using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("LMBackend.Tests")]

namespace LMBackend.RAG.Chroma;

internal class ChromaClient : IChromaClient
{
    private string _baseUrl;
    private HttpClient _httpClient;

    public ChromaClient(HttpClient httpClient)
    {
        string baseUrl = Constants.CHROMA_ENDPOINT;
        _baseUrl = string.IsNullOrEmpty(baseUrl) || baseUrl.EndsWith("/")
            ? baseUrl
            : baseUrl + "/";
        _httpClient = httpClient;
        JsonSerializerSettings = CreateSerializerSettings();
    }

    private static JsonSerializerOptions CreateSerializerSettings()
    {
        var settings = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        return settings;
    }

    protected JsonSerializerOptions JsonSerializerSettings { get; private set; }

    //partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
    //partial void PrepareRequest(HttpClient client, HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
    //partial void ProcessResponse(HttpClient client, HttpResponseMessage response);

    /// <summary>
    /// Retrieves the current user's identity, tenant, and databases.
    /// </summary>
    /// <returns>Get user identity</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<GetUserIdentityResponse> Get_user_identityAsync()
    {
        return Get_user_identityAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves the current user's identity, tenant, and databases.
    /// </summary>
    /// <returns>Get user identity</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<GetUserIdentityResponse> Get_user_identityAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/auth/identity"
                urlBuilder_.Append("api/v2/auth/identity");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<GetUserIdentityResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Health check endpoint that returns 200 if the server and executor are ready
    /// </summary>
    /// <returns>Success</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<string> HealthcheckAsync()
    {
        return HealthcheckAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Health check endpoint that returns 200 if the server and executor are ready
    /// </summary>
    /// <returns>Success</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<string> HealthcheckAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/healthcheck"
                urlBuilder_.Append("api/v2/healthcheck");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<string>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 503)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Service Unavailable", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Heartbeat endpoint that returns a nanosecond timestamp of the current time.
    /// </summary>
    /// <returns>Success</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<HeartbeatResponse> HeartbeatAsync()
    {
        return HeartbeatAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Heartbeat endpoint that returns a nanosecond timestamp of the current time.
    /// </summary>
    /// <returns>Success</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<HeartbeatResponse> HeartbeatAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/heartbeat"
                urlBuilder_.Append("api/v2/heartbeat");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<HeartbeatResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Pre-flight checks endpoint reporting basic readiness info.
    /// </summary>
    /// <returns>Pre flight checks</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<ChecklistResponse> Pre_flight_checksAsync()
    {
        return Pre_flight_checksAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Pre-flight checks endpoint reporting basic readiness info.
    /// </summary>
    /// <returns>Pre flight checks</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<ChecklistResponse> Pre_flight_checksAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/pre-flight-checks"
                urlBuilder_.Append("api/v2/pre-flight-checks");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ChecklistResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Reset endpoint allowing authorized users to reset the database.
    /// </summary>
    /// <returns>Reset successful</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<bool> ResetAsync()
    {
        return ResetAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Reset endpoint allowing authorized users to reset the database.
    /// </summary>
    /// <returns>Reset successful</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<bool> ResetAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/reset"
                urlBuilder_.Append("api/v2/reset");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result_ = (bool)Convert.ChangeType(responseData_, typeof(bool));
                        return result_;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <returns>Tenant created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<CreateTenantResponse> Create_tenantAsync(CreateTenantPayload body)
    {
        return Create_tenantAsync(body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="body"></param>
    /// <returns>Tenant created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<CreateTenantResponse> Create_tenantAsync(CreateTenantPayload body, CancellationToken cancellationToken)
    {
        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants"
                urlBuilder_.Append("api/v2/tenants");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CreateTenantResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Returns an existing tenant by name.
    /// </summary>
    /// <param name="tenant_name">Tenant name or ID to retrieve</param>
    /// <returns>Tenant found</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<GetTenantResponse> Get_tenantAsync(string tenant_name)
    {
        return Get_tenantAsync(tenant_name, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Returns an existing tenant by name.
    /// </summary>
    /// <param name="tenant_name">Tenant name or ID to retrieve</param>
    /// <returns>Tenant found</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<GetTenantResponse> Get_tenantAsync(string tenant_name, CancellationToken cancellationToken)
    {
        if (tenant_name == null)
            throw new ArgumentNullException("tenant_name");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant_name}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant_name, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<GetTenantResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Tenant not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Lists all databases for a given tenant.
    /// </summary>
    /// <param name="tenant">Tenant ID to list databases for</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <returns>List of databases</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<ICollection<Database>> List_databasesAsync(string tenant, int? limit, int? offset)
    {
        return List_databasesAsync(tenant, limit, offset, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Lists all databases for a given tenant.
    /// </summary>
    /// <param name="tenant">Tenant ID to list databases for</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <returns>List of databases</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<ICollection<Database>> List_databasesAsync(string tenant, int? limit, int? offset, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases");
                urlBuilder_.Append('?');
                if (limit != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("limit")).Append('=').Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                if (offset != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("offset")).Append('=').Append(Uri.EscapeDataString(ConvertToString(offset, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                urlBuilder_.Length--;

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ICollection<Database>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Creates a new database for a given tenant.
    /// </summary>
    /// <param name="tenant">Tenant ID to associate with the new database</param>
    /// <param name="body"></param>
    /// <returns>Database created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<CreateDatabaseResponse> Create_databaseAsync(string tenant, CreateDatabasePayload body)
    {
        return Create_databaseAsync(tenant, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Creates a new database for a given tenant.
    /// </summary>
    /// <param name="tenant">Tenant ID to associate with the new database</param>
    /// <param name="body"></param>
    /// <returns>Database created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<CreateDatabaseResponse> Create_databaseAsync(string tenant, CreateDatabasePayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<CreateDatabaseResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Retrieves a specific database by name.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Name of the database to retrieve</param>
    /// <returns>Database retrieved successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<Database> Get_databaseAsync(string tenant, string database)
    {
        return Get_databaseAsync(tenant, database, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves a specific database by name.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Name of the database to retrieve</param>
    /// <returns>Database retrieved successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<Database> Get_databaseAsync(string tenant, string database, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<Database>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Database not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Deletes a specific database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Name of the database to delete</param>
    /// <returns>Database deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<DeleteDatabaseResponse> Delete_databaseAsync(string tenant, string database)
    {
        return Delete_databaseAsync(tenant, database, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Deletes a specific database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Name of the database to delete</param>
    /// <returns>Database deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<DeleteDatabaseResponse> Delete_databaseAsync(string tenant, string database, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("DELETE");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<DeleteDatabaseResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Database not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Lists all collections in the specified database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name to list collections from</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <returns>List of collections</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<ICollection<Collection>> List_collectionsAsync(string tenant, string database, int? limit, int? offset)
    {
        return List_collectionsAsync(tenant, database, limit, offset, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Lists all collections in the specified database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name to list collections from</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <returns>List of collections</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<ICollection<Collection>> List_collectionsAsync(string tenant, string database, int? limit, int? offset, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections");
                urlBuilder_.Append('?');
                if (limit != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("limit")).Append('=').Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                if (offset != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("offset")).Append('=').Append(Uri.EscapeDataString(ConvertToString(offset, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                urlBuilder_.Length--;

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ICollection<Collection>>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Creates a new collection under the specified database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name containing the new collection</param>
    /// <param name="body"></param>
    /// <returns>Collection created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<Collection> Create_collectionAsync(string tenant, string database, CreateCollectionPayload body)
    {
        return Create_collectionAsync(tenant, database, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Creates a new collection under the specified database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name containing the new collection</param>
    /// <param name="body"></param>
    /// <returns>Collection created successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<Collection> Create_collectionAsync(string tenant, string database, CreateCollectionPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<Collection>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Retrieves a collection by ID or name.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection</param>
    /// <returns>Collection found</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<Collection> Get_collectionAsync(string tenant, string database, string collection_id)
    {
        return Get_collectionAsync(tenant, database, collection_id, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves a collection by ID or name.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection</param>
    /// <returns>Collection found</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<Collection> Get_collectionAsync(string tenant, string database, string collection_id, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<Collection>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Updates an existing collection's name or metadata.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to update</param>
    /// <param name="body"></param>
    /// <returns>Collection updated successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<UpdateCollectionResponse> Update_collectionAsync(string tenant, string database, string collection_id, UpdateCollectionPayload body)
    {
        return Update_collectionAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Updates an existing collection's name or metadata.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to update</param>
    /// <param name="body"></param>
    /// <returns>Collection updated successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<UpdateCollectionResponse> Update_collectionAsync(string tenant, string database, string collection_id, UpdateCollectionPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("PUT");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<UpdateCollectionResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Deletes a collection in a given database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to delete</param>
    /// <returns>Collection deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<UpdateCollectionResponse> Delete_collectionAsync(string tenant, string database, string collection_id)
    {
        return Delete_collectionAsync(tenant, database, collection_id, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Deletes a collection in a given database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to delete</param>
    /// <returns>Collection deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<UpdateCollectionResponse> Delete_collectionAsync(string tenant, string database, string collection_id, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("DELETE");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<UpdateCollectionResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Adds records to a collection.
    /// </summary>
    /// <returns>Collection added successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<AddCollectionRecordsResponse> Collection_addAsync(string tenant, string database, string collection_id, AddCollectionRecordsPayload body)
    {
        return Collection_addAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Adds records to a collection.
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="database"></param>
    /// <param name="collection_id"></param>
    /// <param name="body"></param>
    /// <returns>Collection added successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<AddCollectionRecordsResponse> Collection_addAsync(string tenant, string database, string collection_id, AddCollectionRecordsPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/add"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/add");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 201)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<AddCollectionRecordsResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 400)
                    {
                        string responseText_ = response_.Content == null ? string.Empty : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("Invalid data for collection addition", status_, responseText_, headers_, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the number of records in a collection.
    /// </summary>
    /// <param name="tenant">Tenant ID for the collection</param>
    /// <param name="database">Database containing this collection</param>
    /// <param name="collection_id">Collection ID whose records are counted</param>
    /// <returns>Number of records in the collection</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<int> Collection_countAsync(string tenant, string database, string collection_id)
    {
        return Collection_countAsync(tenant, database, collection_id, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves the number of records in a collection.
    /// </summary>
    /// <param name="tenant">Tenant ID for the collection</param>
    /// <param name="database">Database containing this collection</param>
    /// <param name="collection_id">Collection ID whose records are counted</param>
    /// <returns>Number of records in the collection</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<int> Collection_countAsync(string tenant, string database, string collection_id, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/count"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/count");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<int>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Deletes records in a collection. Can filter by IDs or metadata.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">Collection ID</param>
    /// <param name="body"></param>
    /// <returns>Records deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<DeleteCollectionRecordsResponse> Collection_deleteAsync(string tenant, string database, string collection_id, DeleteCollectionRecordsPayload body)
    {
        return Collection_deleteAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Deletes records in a collection. Can filter by IDs or metadata.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">Collection ID</param>
    /// <param name="body"></param>
    /// <returns>Records deleted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<DeleteCollectionRecordsResponse> Collection_deleteAsync(string tenant, string database, string collection_id, DeleteCollectionRecordsPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/delete"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/delete");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<DeleteCollectionRecordsResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Forks an existing collection.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to update</param>
    /// <param name="body"></param>
    /// <returns>Collection forked successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<Collection> Fork_collectionAsync(string tenant, string database, string collection_id, ForkCollectionPayload body)
    {
        return Fork_collectionAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Forks an existing collection.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">UUID of the collection to update</param>
    /// <param name="body"></param>
    /// <returns>Collection forked successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<Collection> Fork_collectionAsync(string tenant, string database, string collection_id, ForkCollectionPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/fork"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/fork");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<Collection>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Retrieves records from a collection by ID or metadata filter.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name for the collection</param>
    /// <param name="collection_id">Collection ID to fetch records from</param>
    /// <param name="body"></param>
    /// <returns>Records retrieved from the collection</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<GetResponse> Collection_getAsync(string tenant, string database, string collection_id, GetRequestPayload body)
    {
        return Collection_getAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves records from a collection by ID or metadata filter.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name for the collection</param>
    /// <param name="collection_id">Collection ID to fetch records from</param>
    /// <param name="body"></param>
    /// <returns>Records retrieved from the collection</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<GetResponse> Collection_getAsync(string tenant, string database, string collection_id, GetRequestPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/get"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/get");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<GetResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Query a collection in a variety of ways, including vector search, metadata filtering, and full-text search
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name containing the collection</param>
    /// <param name="collection_id">Collection ID to query</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="body"></param>
    /// <returns>Records matching the query</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<QueryResponse> Collection_queryAsync(string tenant, string database, string collection_id, int? limit, int? offset, QueryRequestPayload body)
    {
        return Collection_queryAsync(tenant, database, collection_id, limit, offset, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Query a collection in a variety of ways, including vector search, metadata filtering, and full-text search
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name containing the collection</param>
    /// <param name="collection_id">Collection ID to query</param>
    /// <param name="limit">Limit for pagination</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="body"></param>
    /// <returns>Records matching the query</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<QueryResponse> Collection_queryAsync(string tenant, string database, string collection_id, int? limit, int? offset, QueryRequestPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/query"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/query");
                urlBuilder_.Append('?');
                if (limit != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("limit")).Append('=').Append(Uri.EscapeDataString(ConvertToString(limit, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                if (offset != null)
                {
                    urlBuilder_.Append(Uri.EscapeDataString("offset")).Append('=').Append(Uri.EscapeDataString(ConvertToString(offset, System.Globalization.CultureInfo.InvariantCulture))).Append('&');
                }
                urlBuilder_.Length--;

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<QueryResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Updates records in a collection by ID.
    /// </summary>
    /// <returns>Collection updated successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<UpdateCollectionRecordsResponse> Collection_updateAsync(string tenant, string database, string collection_id, UpdateCollectionRecordsPayload body)
    {
        return Collection_updateAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Updates records in a collection by ID.
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="database"></param>
    /// <param name="collection_id"></param>
    /// <param name="body"></param>
    /// <returns>Collection updated successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<UpdateCollectionRecordsResponse> Collection_updateAsync(string tenant, string database, string collection_id, UpdateCollectionRecordsPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/update"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/update");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<UpdateCollectionRecordsResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 404)
                    {
                        string responseText_ = response_.Content == null ? string.Empty : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("Collection not found", status_, responseText_, headers_, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Upserts records in a collection (create if not exists, otherwise update).
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">Collection ID</param>
    /// <param name="body"></param>
    /// <returns>Records upserted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<UpsertCollectionRecordsResponse> Collection_upsertAsync(string tenant, string database, string collection_id, UpsertCollectionRecordsPayload body)
    {
        return Collection_upsertAsync(tenant, database, collection_id, body, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Upserts records in a collection (create if not exists, otherwise update).
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name</param>
    /// <param name="collection_id">Collection ID</param>
    /// <param name="body"></param>
    /// <returns>Records upserted successfully</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<UpsertCollectionRecordsResponse> Collection_upsertAsync(string tenant, string database, string collection_id, UpsertCollectionRecordsPayload body, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        if (collection_id == null)
            throw new ArgumentNullException("collection_id");

        if (body == null)
            throw new ArgumentNullException("body");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                var json_ = JsonSerializer.SerializeToUtf8Bytes(body, JsonSerializerSettings);
                var content_ = new ByteArrayContent(json_);
                content_.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request_.Content = content_;
                request_.Method = new HttpMethod("POST");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections/{collection_id}/upsert"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(collection_id, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/upsert");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<UpsertCollectionRecordsResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 404)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Collection not found", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the total number of collections in a given database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name to count collections from</param>
    /// <returns>Count of collections</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<int> Count_collectionsAsync(string tenant, string database)
    {
        return Count_collectionsAsync(tenant, database, CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Retrieves the total number of collections in a given database.
    /// </summary>
    /// <param name="tenant">Tenant ID</param>
    /// <param name="database">Database name to count collections from</param>
    /// <returns>Count of collections</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<int> Count_collectionsAsync(string tenant, string database, CancellationToken cancellationToken)
    {
        if (tenant == null)
            throw new ArgumentNullException("tenant");

        if (database == null)
            throw new ArgumentNullException("database");

        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/tenants/{tenant}/databases/{database}/collections_count"
                urlBuilder_.Append("api/v2/tenants/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(tenant, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/databases/");
                urlBuilder_.Append(Uri.EscapeDataString(ConvertToString(database, System.Globalization.CultureInfo.InvariantCulture)));
                urlBuilder_.Append("/collections_count");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<int>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        return objectResponse_.Object;
                    }
                    else
                    if (status_ == 401)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Unauthorized", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    if (status_ == 500)
                    {
                        var objectResponse_ = await ReadObjectResponseAsync<ErrorResponse>(response_, headers_, cancellationToken).ConfigureAwait(false);
                        if (objectResponse_.Object == null)
                        {
                            throw new ApiException("Response was null which was not expected.", status_, objectResponse_.Text, headers_, null);
                        }
                        throw new ApiException<ErrorResponse>("Server error", status_, objectResponse_.Text, headers_, objectResponse_.Object, null);
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    /// <summary>
    /// Returns the version of the server.
    /// </summary>
    /// <returns>Get server version</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public Task<string> VersionAsync()
    {
        return VersionAsync(CancellationToken.None);
    }

    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <summary>
    /// Returns the version of the server.
    /// </summary>
    /// <returns>Get server version</returns>
    /// <exception cref="ApiException">A server side error occurred.</exception>
    public async Task<string> VersionAsync(CancellationToken cancellationToken)
    {
        var client_ = _httpClient;
        var disposeClient_ = false;
        try
        {
            using (var request_ = new HttpRequestMessage())
            {
                request_.Method = new HttpMethod("GET");
                request_.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/plain"));

                var urlBuilder_ = new System.Text.StringBuilder();
                if (!string.IsNullOrEmpty(_baseUrl)) urlBuilder_.Append(_baseUrl);
                // Operation Path: "api/v2/version"
                urlBuilder_.Append("api/v2/version");

                //PrepareRequest(client_, request_, urlBuilder_);

                var url_ = urlBuilder_.ToString();
                request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

                //PrepareRequest(client_, request_, url_);

                var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                var disposeResponse_ = true;
                try
                {
                    var headers_ = new Dictionary<string, IEnumerable<string>>();
                    foreach (var item_ in response_.Headers)
                        headers_[item_.Key] = item_.Value;
                    if (response_.Content != null && response_.Content.Headers != null)
                    {
                        foreach (var item_ in response_.Content.Headers)
                            headers_[item_.Key] = item_.Value;
                    }

                    //ProcessResponse(client_, response_);

                    var status_ = (int)response_.StatusCode;
                    if (status_ == 200)
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result_ = (string)Convert.ChangeType(responseData_, typeof(string));
                        return result_;
                    }
                    else
                    {
                        var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new ApiException("The HTTP status code of the response was not expected (" + status_ + ").", status_, responseData_, headers_, null);
                    }
                }
                finally
                {
                    if (disposeResponse_)
                        response_.Dispose();
                }
            }
        }
        finally
        {
            if (disposeClient_)
                client_.Dispose();
        }
    }

    protected struct ObjectResponseResult<T>
    {
        public ObjectResponseResult(T responseObject, string responseText)
        {
            Object = responseObject;
            Text = responseText;
        }

        public T Object { get; }

        public string Text { get; }
    }

    public bool ReadResponseAsString { get; set; }

    protected virtual async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(HttpResponseMessage response, IReadOnlyDictionary<string, IEnumerable<string>> headers, CancellationToken cancellationToken)
    {
        if (response == null || response.Content == null)
        {
            return new ObjectResponseResult<T>(default, string.Empty);
        }

        if (ReadResponseAsString)
        {
            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                var typedBody = JsonSerializer.Deserialize<T>(responseText, JsonSerializerSettings);
                return new ObjectResponseResult<T>(typedBody, responseText);
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
            }
        }
        else
        {
            try
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    var typedBody = await JsonSerializer.DeserializeAsync<T>(responseStream, JsonSerializerSettings, cancellationToken).ConfigureAwait(false);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
            }
            catch (JsonException exception)
            {
                var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
            }
        }
    }

    private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
    {
        if (value == null)
        {
            return "";
        }

        if (value is Enum)
        {
            var name = Enum.GetName(value.GetType(), value);
            if (name != null)
            {
                var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                if (field != null)
                {
                    var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(EnumMemberAttribute))
                        as EnumMemberAttribute;
                    if (attribute != null)
                    {
                        return attribute.Value != null ? attribute.Value : name;
                    }
                }

                var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                return converted == null ? string.Empty : converted;
            }
        }
        else if (value is bool)
        {
            return Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
        }
        else if (value is byte[])
        {
            return Convert.ToBase64String((byte[])value);
        }
        else if (value is string[])
        {
            return string.Join(",", (string[])value);
        }
        else if (value.GetType().IsArray)
        {
            var valueArray = (Array)value;
            var valueTextArray = new string[valueArray.Length];
            for (var i = 0; i < valueArray.Length; i++)
            {
                valueTextArray[i] = ConvertToString(valueArray.GetValue(i), cultureInfo);
            }
            return string.Join(",", valueTextArray);
        }

        var result = Convert.ToString(value, cultureInfo);
        return result == null ? "" : result;
    }
}


internal class AddCollectionRecordsPayload
{

    [JsonPropertyName("documents")]
    public ICollection<string> Documents { get; set; }

    [JsonPropertyName("embeddings")]
    public ICollection<ICollection<float>> Embeddings { get; set; }

    [JsonPropertyName("ids")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<string> Ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

    [JsonPropertyName("metadatas")]
    public ICollection<Dictionary<string, object>> Metadatas { get; set; }

    [JsonPropertyName("uris")]
    public ICollection<string> Uris { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class AddCollectionRecordsResponse
{

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class ChecklistResponse
{

    [JsonPropertyName("max_batch_size")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int Max_batch_size { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class Collection
{

    [JsonPropertyName("configuration_json")]
    [System.ComponentModel.DataAnnotations.Required]
    public CollectionConfiguration Configuration_json { get; set; } = new CollectionConfiguration();

    [JsonPropertyName("database")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Database { get; set; }

    [JsonPropertyName("dimension")]
    public int? Dimension { get; set; }

    [JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("log_position")]
    public long Log_position { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, bool> Metadata { get; set; }

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [JsonPropertyName("tenant")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Tenant { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class CollectionConfiguration
{

    //[JsonPropertyName("embedding_function")]
    //public Embedding_function Embedding_function { get; set; }

    [JsonPropertyName("hnsw")]
    public HnswConfiguration Hnsw { get; set; }

    [JsonPropertyName("spann")]
    public SpannConfiguration Spann { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class CreateCollectionPayload
{

    [JsonPropertyName("configuration")]
    public CollectionConfiguration Configuration { get; set; }

    [JsonPropertyName("get_or_create")]
    public bool Get_or_create { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, bool> Metadata { get; set; }

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class CreateDatabasePayload
{

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class CreateDatabaseResponse
{

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class CreateTenantPayload
{

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class CreateTenantResponse
{
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class Database
{

    [JsonPropertyName("id")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    [JsonPropertyName("tenant")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Tenant { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class DeleteCollectionRecordsPayload : RawWhereFields
{

    [JsonPropertyName("ids")]
    public ICollection<string> Ids { get; set; }

}


public class DeleteCollectionRecordsResponse
{
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class DeleteDatabaseResponse
{
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


/*internal class EmbeddingFunctionConfiguration
{

    private IDictionary<string, object> _additionalProperties;

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties
    {
        get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
        set { _additionalProperties = value; }
    }

}


internal class EmbeddingFunctionNewConfiguration
{

    [JsonPropertyName("config")]
    [System.ComponentModel.DataAnnotations.Required]
    public object Config { get; set; }

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    private IDictionary<string, object> _additionalProperties;

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties
    {
        get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
        set { _additionalProperties = value; }
    }

}*/


internal class ErrorResponse
{

    [JsonPropertyName("error")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Error { get; set; }

    [JsonPropertyName("message")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Message { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class ForkCollectionPayload
{

    [JsonPropertyName("new_name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string New_name { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class GetRequestPayload : RawWhereFields
{

    [JsonPropertyName("ids")]
    public ICollection<string> Ids { get; set; }

    [JsonPropertyName("include")]

    // TODO(system.text.json): Add string enum item converter
    public Include[] Include { get; set; }

    [JsonPropertyName("limit")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Offset { get; set; }

}


internal class GetResponse
{

    [JsonPropertyName("documents")]
    public ICollection<string> Documents { get; set; }

    [JsonPropertyName("embeddings")]
    public ICollection<ICollection<float>> Embeddings { get; set; }

    [JsonPropertyName("ids")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<string> Ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

    [JsonPropertyName("include")]

    // TODO(system.text.json): Add string enum item converter
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<Include> Include { get; set; } = new System.Collections.ObjectModel.Collection<Include>();

    [JsonPropertyName("metadatas")]
    public ICollection<Dictionary<string, object>> Metadatas { get; set; }

    [JsonPropertyName("uris")]
    public ICollection<string> Uris { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class GetTenantResponse
{

    [JsonPropertyName("name")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Name { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class GetUserIdentityResponse
{

    [JsonPropertyName("databases")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<string> Databases { get; set; } = new System.Collections.ObjectModel.Collection<string>();

    [JsonPropertyName("tenant")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string Tenant { get; set; }

    [JsonPropertyName("user_id")]
    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
    public string User_id { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


//internal class HashMap : Dictionary<string, bool>
//{

//}


internal class HeartbeatResponse
{

    [JsonPropertyName("nanosecond heartbeat")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int Nanosecond_heartbeat { get; set; }

    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class HnswConfiguration
{

    [JsonPropertyName("ef_construction")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Ef_construction { get; set; }

    [JsonPropertyName("ef_search")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Ef_search { get; set; }

    [JsonPropertyName("max_neighbors")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Max_neighbors { get; set; }

    [JsonPropertyName("resize_factor")]
    public double? Resize_factor { get; set; }

    [JsonPropertyName("space")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HnswSpace Space { get; set; }

    [JsonPropertyName("sync_threshold")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Sync_threshold { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HnswSpace
{

    [EnumMember(Value = @"l2")]
    L2 = 0,

    [EnumMember(Value = @"cosine")]
    Cosine = 1,

    [EnumMember(Value = @"ip")]
    Ip = 2,

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Include
{

    [EnumMember(Value="distances")]
    Distances = 0,

    [EnumMember(Value="documents")]
    Documents = 1,

    [EnumMember(Value="embeddings")]
    Embeddings = 2,

    [EnumMember(Value="metadatas")]
    Metadatas = 3,

    [EnumMember(Value="uris")]
    Uris = 4,

}


//internal class IncludeList : System.Collections.ObjectModel.Collection<Include>
//{

//}


public class QueryRequestPayload : RawWhereFields
{

    [JsonPropertyName("ids")]
    public ICollection<string> Ids { get; set; }

    [JsonPropertyName("include")]

    // TODO(system.text.json): Add string enum item converter
    public Include[] Include { get; set; }

    [JsonPropertyName("n_results")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? N_results { get; set; }

    [JsonPropertyName("query_embeddings")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<ICollection<float>> Query_embeddings { get; set; } = new System.Collections.ObjectModel.Collection<ICollection<float>>();

}


public class QueryResponse
{

    [JsonPropertyName("distances")]
    public ICollection<ICollection<float?>> Distances { get; set; }

    [JsonPropertyName("documents")]
    public ICollection<ICollection<string>> Documents { get; set; }

    [JsonPropertyName("embeddings")]
    public ICollection<ICollection<ICollection<float>>> Embeddings { get; set; }

    [JsonPropertyName("ids")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<ICollection<string>> Ids { get; set; } = new System.Collections.ObjectModel.Collection<ICollection<string>>();

    [JsonPropertyName("include")]

    // TODO(system.text.json): Add string enum item converter
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<Include> Include { get; set; } = new System.Collections.ObjectModel.Collection<Include>();

    [JsonPropertyName("metadatas")]
    public ICollection<ICollection<Dictionary<string, object>>> Metadatas { get; set; }

    [JsonPropertyName("uris")]
    public ICollection<ICollection<string>> Uris { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class RawWhereFields
{

    [JsonPropertyName("where")]
    public object Where { get; set; }

    [JsonPropertyName("where_document")]
    public object Where_document { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class SpannConfiguration
{

    [JsonPropertyName("ef_construction")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Ef_construction { get; set; }

    [JsonPropertyName("ef_search")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Ef_search { get; set; }

    [JsonPropertyName("max_neighbors")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Max_neighbors { get; set; }

    [JsonPropertyName("merge_threshold")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Merge_threshold { get; set; }

    [JsonPropertyName("reassign_neighbor_count")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Reassign_neighbor_count { get; set; }

    [JsonPropertyName("search_nprobe")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Search_nprobe { get; set; }

    [JsonPropertyName("space")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HnswSpace Space { get; set; }

    [JsonPropertyName("split_threshold")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Split_threshold { get; set; }

    [JsonPropertyName("write_nprobe")]
    [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
    public int? Write_nprobe { get; set; }

}


internal class UpdateCollectionConfiguration
{

    //[JsonPropertyName("embedding_function")]
    //public Embedding_function Embedding_function { get; set; }

    [JsonPropertyName("hnsw")]
    public UpdateHnswConfiguration Hnsw { get; set; }

    [JsonPropertyName("spann")]
    public SpannConfiguration Spann { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class UpdateCollectionPayload
{

    [JsonPropertyName("new_configuration")]
    public UpdateCollectionConfiguration New_configuration { get; set; }

    [JsonPropertyName("new_metadata")]
    public Dictionary<string, bool> New_metadata { get; set; }

    [JsonPropertyName("new_name")]
    public string New_name { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class UpdateCollectionRecordsPayload
{

    [JsonPropertyName("documents")]
    public ICollection<string> Documents { get; set; }

    [JsonPropertyName("embeddings")]
    public ICollection<ICollection<float>> Embeddings { get; set; }

    [JsonPropertyName("ids")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<string> Ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

    [JsonPropertyName("metadatas")]
    public ICollection<Dictionary<string, object>> Metadatas { get; set; }

    [JsonPropertyName("uris")]
    public ICollection<string> Uris { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class UpdateCollectionRecordsResponse
{

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class UpdateCollectionResponse
{

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


internal class UpdateHnswConfiguration
{

    [JsonPropertyName("batch_size")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Batch_size { get; set; }

    [JsonPropertyName("ef_search")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Ef_search { get; set; }

    [JsonPropertyName("max_neighbors")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Max_neighbors { get; set; }

    [JsonPropertyName("num_threads")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Num_threads { get; set; }

    [JsonPropertyName("resize_factor")]
    public double? Resize_factor { get; set; }

    [JsonPropertyName("sync_threshold")]
    [System.ComponentModel.DataAnnotations.Range(0D, double.MaxValue)]
    public int? Sync_threshold { get; set; }

}


public class UpsertCollectionRecordsPayload
{

    [JsonPropertyName("documents")]
    public ICollection<string> Documents { get; set; }

    [JsonPropertyName("embeddings")]
    public ICollection<float[]> Embeddings { get; set; }

    [JsonPropertyName("ids")]
    [System.ComponentModel.DataAnnotations.Required]
    public ICollection<string> Ids { get; set; } = new System.Collections.ObjectModel.Collection<string>();

    [JsonPropertyName("metadatas")]
    public ICollection<Dictionary<string, object>> Metadatas { get; set; }

    [JsonPropertyName("uris")]
    public ICollection<string> Uris { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


public class UpsertCollectionRecordsResponse
{
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; }
}


//internal class Vec : System.Collections.ObjectModel.Collection<Anonymous>
//{

//}


//internal class Anonymous
//{

//    [JsonPropertyName("configuration_json")]
//    [System.ComponentModel.DataAnnotations.Required]
//    public CollectionConfiguration Configuration_json { get; set; } = new CollectionConfiguration();

//    [JsonPropertyName("database")]
//    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
//    public string Database { get; set; }

//    [JsonPropertyName("dimension")]
//    public int? Dimension { get; set; }

//    [JsonPropertyName("id")]
//    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
//    public System.Guid Id { get; set; }

//    [JsonPropertyName("log_position")]
//    public long Log_position { get; set; }

//    [JsonPropertyName("metadata")]
//    public Dictionary<string, bool> Metadata { get; set; }

//    [JsonPropertyName("name")]
//    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
//    public string Name { get; set; }

//    [JsonPropertyName("tenant")]
//    [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
//    public string Tenant { get; set; }

//    [JsonPropertyName("version")]
//    public int Version { get; set; }

//    private IDictionary<string, object> _additionalProperties;

//    [JsonExtensionData]
//    public IDictionary<string, object> AdditionalProperties
//    {
//        get { return _additionalProperties ?? (_additionalProperties = new Dictionary<string, object>()); }
//        set { _additionalProperties = value; }
//    }

//}


/*public enum EmbeddingFunctionConfigurationType
{

    [EnumMember(Value = @"legacy")]
    Legacy = 0,

}*/




internal class ApiException : Exception
{
    public int StatusCode { get; private set; }

    public string Response { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
        : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + (response == null ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers;
    }

    public override string ToString()
    {
        return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
    }
}

internal class ApiException<TResult> : ApiException
{
    public TResult Result { get; private set; }

    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result, Exception innerException)
        : base(message, statusCode, response, headers, innerException)
    {
        Result = result;
    }
}