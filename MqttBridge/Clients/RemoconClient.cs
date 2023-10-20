using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using MqttBridge.Configuration;
using MqttBridge.Models.Input.RemoconApi;

namespace MqttBridge.Clients;

public class RemoconClient : AuthenticatingRestClient
{
    private readonly RemoconSettings _settings;

    private LoginResponse? _currentLogin;

    public RemoconClient(IOptions<RemoconSettings> settings) : base(settings.Value.BaseUrl)
    {
        _settings = settings.Value;
    }

    public Task Login(CancellationToken cancellationToken)
    {
        return RefreshAuthentication(cancellationToken);
    }

    protected override async Task RefreshAuthentication(CancellationToken cancellationToken)
    {
        _currentLogin = await Login(_settings.User, _settings.Password, cancellationToken);
    }

    protected async Task<LoginResponse?> Login(string username, string password, CancellationToken cancellationToken)
    {
        LoginRequest request = new(username, password);
        return await UnauthenticatedPost<LoginRequest, LoginResponse>(request, "accounts/login", cancellationToken);
    }

    public async Task<IReadOnlyList<Plant>> GetPlants(CancellationToken cancellationToken)
    {
        IReadOnlyList<Plant>? response = await Get<List<Plant>>("remote/plants/lite", cancellationToken);
        return response ?? ImmutableArray<Plant>.Empty;
    }

    public async Task<PlantData?> GetPlantData(string gatewayId, CancellationToken cancellationToken)
    {
        return await Get<PlantData>($"remote/bsbPlantData/{gatewayId}?fetchFlame=true&fetchHpOn=true", cancellationToken);
    }

    public async Task<RemotePlantFeatures?> GetPlantFeatures(string gatewayId, CancellationToken cancellationToken)
    {
        return await Get<RemotePlantFeatures>($"remote/plants/{gatewayId}/features?eagerMode=true", cancellationToken);
    }

    public async Task<DataItemsResponse?> GetPlantItems(string gatewayId, RemotePlantFeatures features, CancellationToken cancellationToken)
    {
        List<DataItemIdentifier> items = new();

        foreach (Zone zone in features.Zones)
        {
            items.Add(new DataItemIdentifier(DataItemIds.ChFlowTemp.ToString("G"), zone.Number));
            items.Add(new DataItemIdentifier(DataItemIds.HeatingCircuitPressure.ToString("G"), zone.Number));
            items.Add(new DataItemIdentifier(DataItemIds.IsFlameOn.ToString("G"), zone.Number));
            items.Add(new DataItemIdentifier(DataItemIds.HeatingCircuitPressure.ToString("G"), zone.Number));
        }

        DataItemsRequest request = new(false, items, features, "de");
        return await Post<DataItemsRequest, DataItemsResponse>(request, $"remote/dataItems/{gatewayId}/get?umsys=si", cancellationToken);
    }


    protected override async Task AuthenticateRequest(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        if (_currentLogin == null)
        {
            await RefreshAuthentication(cancellationToken);
        }

        httpRequestMessage.Headers.Add("ar.authToken", _currentLogin!.Token);
    }
}