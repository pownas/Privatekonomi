# .NET Aspire Orchestrator Guide

Detta dokument beskriver hur du använder .NET Aspire Orchestrator för att köra och hantera Privatekonomi-applikationen.

## Vad är .NET Aspire?

.NET Aspire är ett opinionated, cloud-ready stack för att bygga observerbara, produktionsklara, distribuerade applikationer. Aspire förenklar utvecklingsupplevelsen genom att:

- **Orkestrera** flera tjänster och beroenden
- **Övervaka** applikationer med inbyggd telemetri och observerbarhet
- **Förenkla** konfiguration och service discovery
- **Standardisera** best practices för cloud-native applikationer

## Arkitektur

Privatekonomi-lösningen har nu följande Aspire-komponenter:

### 1. Privatekonomi.AppHost
**AppHost** är orchestrator-projektet som:
- Koordinerar körning av alla tjänster
- Hanterar service discovery
- Tillhandahåller Aspire Dashboard för övervakning
- Konfigurerar kommunikation mellan tjänster

### 2. Privatekonomi.ServiceDefaults
**ServiceDefaults** är ett delat bibliotek som:
- Konfigurerar OpenTelemetry för telemetri och observerbarhet
- Lägger till health checks
- Aktiverar service discovery
- Konfigurerar resilience patterns (retry, circuit breaker, etc.)

### 3. Orchestrerade tjänster
- **Privatekonomi.Web** - Blazor Server-applikation
- **Privatekonomi.Api** - ASP.NET Core Web API

## Förutsättningar

För att köra Aspire Orchestrator behöver du:

1. **.NET 9 SDK**
   ```bash
   dotnet --version  # Ska visa 9.0 eller högre
   ```

2. **Aspire workload**
   ```bash
   dotnet workload install aspire
   ```

3. **Docker Desktop** (för lokal utveckling)
   - Krävs för DCP (Distributed Control Plane)
   - Ladda ner från: https://www.docker.com/products/docker-desktop

## Komma igång

### Starta applikationen med Aspire

1. Öppna terminalen i projektets rotkatalog
2. Kör AppHost-projektet:
   ```bash
   cd src/Privatekonomi.AppHost
   dotnet run
   ```

3. Aspire Dashboard öppnas automatiskt i din webbläsare (vanligtvis på `http://localhost:15000`)

### Aspire Dashboard

Dashboard ger dig:
- **Översikt** över alla tjänster och deras status
- **Logs** från alla tjänster i realtid
- **Traces** för att följa requests genom systemet
- **Metrics** för prestanda och hälsa
- **Miljövariabler** och konfiguration

### Åtkomst till tjänsterna

När Aspire kör:
- **Web UI**: Tillgänglig på den port som visas i Dashboard
- **API**: Tillgänglig på den port som visas i Dashboard
- **Swagger**: `<api-url>/swagger`

## Tjänstekonfiguration

### ServiceDefaults inkluderar:

#### OpenTelemetry
- Automatisk instrumentering av HTTP-requests
- Logging med strukturerad data
- Metrics för ASP.NET Core och runtime
- Traces för distribuerad spårning

#### Health Checks
- `/health` - Kontrollerar om alla health checks passerar
- `/alive` - Kontrollerar grundläggande liveness

#### Resilience
- Automatiska retries vid misslyckade requests
- Circuit breaker pattern
- Timeout-hantering

#### Service Discovery
- Automatisk upptäckt av tjänster
- Enkel konfiguration av tjänst-till-tjänst kommunikation

## Utveckling

### Lägga till en ny tjänst

1. Lägg till projektreferens i `Privatekonomi.AppHost.csproj`:
   ```xml
   <ProjectReference Include="..\MinNyaTjänst\MinNyaTjänst.csproj" />
   ```

2. Registrera tjänsten i `AppHost/Program.cs`:
   ```csharp
   var nyTjänst = builder.AddProject<Projects.MinNyaTjänst>("mintjanst");
   ```

3. Lägg till ServiceDefaults i din nya tjänst:
   ```csharp
   // I Program.cs
   builder.AddServiceDefaults();
   
   // Efter app.Build()
   app.MapDefaultEndpoints();
   ```

### Tjänstberoenden

För att en tjänst ska kunna kommunicera med en annan:

```csharp
var api = builder.AddProject<Projects.Privatekonomi_Api>("api");
var web = builder.AddProject<Projects.Privatekonomi_Web>("web")
    .WithReference(api);  // Web kan nu upptäcka och anropa API
```

## Best Practices

1. **Använd alltid ServiceDefaults** i alla tjänster för konsekvent observerbarhet
2. **Konfigurera health checks** för kritiska beroenden
3. **Använd strukturerad logging** för bättre felsökning
4. **Övervaka metrics** i Dashboard regelbundet
5. **Testa lokalt med Aspire** före deployment

## Felsökning

### "Docker is not running"
- Starta Docker Desktop
- Kontrollera att Docker daemon körs: `docker ps`

### "DCP not found"
- Installera/uppdatera Aspire workload: `dotnet workload install aspire`
- Starta om IDE/terminal efter installation

### Tjänst startar inte
- Kontrollera logs i Aspire Dashboard
- Verifiera att alla NuGet-paket är installerade: `dotnet restore`
- Kontrollera att portar inte är upptagna

### Health checks misslyckas
- Kontrollera `/health` endpoint direkt i webbläsare
- Verifiera databas-connections och andra beroenden
- Granska logs för specifika fel

## Resurser

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Samples](https://github.com/dotnet/aspire-samples)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

## Nästa steg

- [ ] Lägg till Redis för caching (Aspire har inbyggt stöd)
- [ ] Integrera med SQL Server istället för InMemory databas
- [ ] Konfigurera Azure deployment med Aspire
- [ ] Lägg till message queue (t.ex. RabbitMQ) för bakgrundsprocesser
