# Issue 5: Förbättra Inloggningssida med Illustration och Moderniserat Formulär

**Labels:** `design`, `authentication`, `ux`, `medium-priority`, `fas-2`

**Prioritet:** ⭐⭐ Medel

**Estimat:** 2-3 dagar

**Fas:** Fas 2 - Visuella Förbättringar

---

## Beskrivning

Förbättra inloggningssidan med en välkomnande illustration/grafik, förbättrad formulärdesign och bättre användning av skärmutrymme för ett professionellare första intryck som stärker varumärket och ökar användarnas förtroende.

## Bakgrund

- Inloggningssidan är ren och funktionell men kan vara mer inbjudande
- Saknar visuellt intresse och varumärkesidentitet
- Använder inte skärmutrymmet effektivt på desktop
- Första intrycket är viktigt för användarnas förtroende
- Kan förbättras med modern split-screen design

## Åtgärder

### Fas 5a: Illustration och Layout
- [ ] Skapa/välj illustration som representerar "ekonomisk frihet" eller "ta kontroll över ekonomi"
- [ ] Implementera split-screen layout (illustration + formulär) på desktop
- [ ] Dölj illustration på mobil för att spara utrymme (responsivt)
- [ ] Lägg till välkomnande text och tagline på illustrationssidan
- [ ] Använd primärfärg (#6366F1) för gradient-bakgrund på illustrationssidan
- [ ] Lägg till logotyp/appnamn prominent

### Fas 5b: Formulärförbättringar
- [ ] Centrera och begränsa formulärbredd (max-width: 400px)
- [ ] Öka padding och spacing mellan fält (24px mellan fält)
- [ ] Förstora inloggningsknappen (height: 48px, font-weight: 600)
- [ ] Lägg till fokusanimationer på formulärfält
- [ ] Använd Variant.Outlined för textfält för modernare look
- [ ] Lägg till ikoner i textfält (Email, Lock)
- [ ] Förbättra "Kom ihåg mig" checkbox layout

### Fas 5c: Varumärkesidentitet
- [ ] Lägg till logotyp eller app-ikon i header
- [ ] Använd primärfärg för accenter konsekvent
- [ ] Säkerställ konsekvent typografi (Inter font)
- [ ] Lägg till subtle shadow på formulär-container
- [ ] Implementera smooth transitions på fokus

## Teknisk Implementation

### Login.razor (uppdaterad)

```razor
@page "/Account/Login"
@using System.ComponentModel.DataAnnotations
@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Identity
@inject SignInManager<ApplicationUser> SignInManager
@inject NavigationManager NavigationManager

<PageTitle>Logga in - Privatekonomi</PageTitle>

<MudGrid Class="login-page" Spacing="0">
    <!-- Illustration Section (endast desktop) -->
    <MudItem xs="12" md="6" Class="login-illustration d-none d-md-flex">
        <div class="illustration-content">
            <img src="/images/login-illustration.svg" 
                 alt="Ta kontroll över din ekonomi" 
                 class="illustration-image" />
            <MudText Typo="Typo.h4" Class="illustration-title">
                Ta kontroll över din ekonomi
            </MudText>
            <MudText Typo="Typo.body1" Class="illustration-subtitle">
                Spara tid, pengar och få bättre koll med Privatekonomi. 
                Din personliga ekonomiassistent som hjälper dig att nå dina mål.
            </MudText>
            <div class="illustration-features">
                <div class="feature-item">
                    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" />
                    <MudText>Automatisk kategorisering</MudText>
                </div>
                <div class="feature-item">
                    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" />
                    <MudText>Budgetplanering</MudText>
                </div>
                <div class="feature-item">
                    <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" />
                    <MudText>Sparmål & investeringar</MudText>
                </div>
            </div>
        </div>
    </MudItem>
    
    <!-- Form Section -->
    <MudItem xs="12" md="6" Class="login-form-container">
        <div class="login-form">
            <!-- Logo -->
            <div class="login-logo">
                <MudImage Src="/images/logo.svg" Alt="Privatekonomi" Width="60" Class="mb-2" />
                <MudText Typo="Typo.h5" Color="Color.Primary" Class="mb-6">
                    <strong>Privatekonomi</strong>
                </MudText>
            </div>
            
            <MudText Typo="Typo.h5" Class="mb-2">Välkommen tillbaka</MudText>
            <MudText Typo="Typo.body2" Class="text-muted mb-6">
                Logga in på ditt konto för att fortsätta
            </MudText>
            
            <EditForm Model="@loginModel" OnValidSubmit="HandleLogin">
                <DataAnnotationsValidator />
                
                <MudTextField Label="E-postadress" 
                              @bind-Value="loginModel.Email" 
                              For="@(() => loginModel.Email)"
                              Variant="Variant.Outlined"
                              Class="mb-4"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Filled.Email"
                              AdornmentColor="Color.Primary"
                              FullWidth="true" />
                
                <MudTextField Label="Lösenord" 
                              @bind-Value="loginModel.Password" 
                              For="@(() => loginModel.Password)"
                              Variant="Variant.Outlined"
                              InputType="InputType.Password"
                              Class="mb-4"
                              Adornment="Adornment.Start"
                              AdornmentIcon="@Icons.Material.Filled.Lock"
                              AdornmentColor="Color.Primary"
                              FullWidth="true" />
                
                <div class="login-options mb-4">
                    <MudCheckBox @bind-Value="loginModel.RememberMe" 
                                 Label="Kom ihåg mig" 
                                 Color="Color.Primary" />
                    <MudLink Href="/Account/ForgotPassword" Typo="Typo.body2">
                        Glömt lösenord?
                    </MudLink>
                </div>
                
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           FullWidth="true"
                           Size="Size.Large"
                           ButtonType="ButtonType.Submit"
                           Class="login-button">
                    Logga in
                </MudButton>
            </EditForm>
            
            <MudDivider Class="my-6" />
            
            <MudText Typo="Typo.body2" Align="Align.Center" Class="text-muted">
                Har du inget konto? 
                <MudLink Href="/Account/Register" Typo="Typo.body2" Color="Color.Primary">
                    <strong>Registrera dig här</strong>
                </MudLink>
            </MudText>
        </div>
    </MudItem>
</MudGrid>

@code {
    private LoginModel loginModel = new();

    private class LoginModel
    {
        [Required(ErrorMessage = "E-postadress krävs")]
        [EmailAddress(ErrorMessage = "Ogiltig e-postadress")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lösenord krävs")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    private async Task HandleLogin()
    {
        // Login logic here
    }
}
```

### CSS-stilar

```css
/* Login page layout */
.login-page {
    min-height: 100vh;
    margin: 0;
}

/* Illustration section */
.login-illustration {
    background: linear-gradient(135deg, #6366F1 0%, #8B5CF6 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 48px;
    position: relative;
    overflow: hidden;
}

/* Subtle pattern overlay */
.login-illustration::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-image: radial-gradient(circle, rgba(255,255,255,0.1) 1px, transparent 1px);
    background-size: 20px 20px;
    opacity: 0.5;
}

.illustration-content {
    text-align: center;
    color: white;
    max-width: 500px;
    position: relative;
    z-index: 1;
}

.illustration-image {
    max-width: 350px;
    width: 100%;
    margin-bottom: 32px;
    filter: drop-shadow(0 10px 30px rgba(0, 0, 0, 0.2));
}

.illustration-title {
    font-weight: 700;
    margin-bottom: 16px;
    color: white;
}

.illustration-subtitle {
    opacity: 0.95;
    line-height: 1.6;
    margin-bottom: 32px;
}

.illustration-features {
    display: flex;
    flex-direction: column;
    gap: 12px;
    text-align: left;
}

.feature-item {
    display: flex;
    align-items: center;
    gap: 12px;
}

.feature-item .mud-icon-root {
    font-size: 20px;
}

/* Form section */
.login-form-container {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 48px 24px;
    background: var(--mud-palette-surface);
}

.login-form {
    max-width: 420px;
    width: 100%;
}

.login-logo {
    text-align: center;
    margin-bottom: 24px;
}

.login-options {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.login-button {
    height: 48px;
    font-size: 1rem;
    font-weight: 600;
    text-transform: none;
}

/* Focus animation for input fields */
.login-form .mud-input-outlined {
    transition: box-shadow var(--transition-base);
}

.login-form .mud-input-outlined:focus-within {
    box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
}

/* Dark mode adjustments */
.mud-theme-dark .login-form-container {
    background: var(--mud-palette-background);
}

/* Responsiv design */
@media (max-width: 960px) {
    .login-form-container {
        padding: 32px 16px;
    }
    
    .login-form {
        max-width: 100%;
    }
}

@media (max-width: 600px) {
    .login-button {
        height: 44px;
    }
}

/* Animation för entrance */
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.login-form {
    animation: fadeInUp 0.5s ease-out;
}

/* Respektera prefers-reduced-motion */
@media (prefers-reduced-motion: reduce) {
    .login-form {
        animation: none;
    }
}
```

### Illustration Placeholder

För utvecklingsmiljön, om illustration inte finns tillgänglig än:

```html
<!-- Placeholder i wwwroot/images/login-illustration.svg -->
<!-- Använd en enkel SVG eller en placeholder från Undraw.co eller storyset.com -->
<!-- Rekommenderade teman: "Personal Finance", "Savings", "Goals", "Security" -->
```

## Berörd Kod

### Filer som ska modifieras
- `src/Privatekonomi.Web/Components/Pages/Account/Login.razor`
  - Implementera split-screen layout
  - Uppdatera formulär med nya stilar
  - Lägg till illustration-sektion
  - Förbättra typografi och spacing

- `src/Privatekonomi.Web/wwwroot/app.css`
  - Lägg till login-page stilar
  - Illustration-section gradient och layout
  - Form-container och responsiv design
  - Focus-animationer

### Nya resurser att lägga till
- `src/Privatekonomi.Web/wwwroot/images/login-illustration.svg`
  - Välkomnande illustration (kan användas från Undraw.co eller liknande)
  - Rekommenderad storlek: 400x300px
  - Format: SVG för skalbarhet

- `src/Privatekonomi.Web/wwwroot/images/logo.svg` (om den inte finns)
  - App-logotyp eller ikon
  - Storlek: 60x60px

## Acceptanskriterier

- [ ] Inloggningssidan har split-screen layout på desktop (md och större)
- [ ] Illustration och välkomnande text visas på vänster sida
- [ ] Gradient-bakgrund (#6366F1 → #8B5CF6) används för illustrationssida
- [ ] Feature-lista visas under illustration
- [ ] Formuläret är centrerat med max-width 420px
- [ ] Textfält använder Variant.Outlined med ikoner
- [ ] Inloggningsknappen är 48px hög med font-weight 600
- [ ] Fokus-animationer fungerar på textfält
- [ ] "Kom ihåg mig" och "Glömt lösenord" är väl layoutade
- [ ] Responsiv design fungerar på mobil (utan illustration)
- [ ] Illustration döljs på skärmar < 960px bredd
- [ ] Logotyp visas prominent i formulär-header
- [ ] Registreringslänk är tydlig och stilmässigt konsekvent
- [ ] Dark mode fungerar korrekt
- [ ] Entrance-animation fungerar smidigt
- [ ] Animation respekterar `prefers-reduced-motion`
- [ ] WCAG AA kontrast-krav uppfylls

## Illustration Rekommendationer

### Teman att överväga:
1. **Personal Finance** - Person med graf och mynt
2. **Savings Goal** - Person som når upp till mål
3. **Financial Security** - Skyddad ekonomi/säkerhet
4. **Budget Planning** - Person med kalkylator/planering

### Föreslagna källor:
- [Undraw.co](https://undraw.co/) - Gratis, anpassningsbara illustrationer
- [Storyset.com](https://storyset.com/) - Gratis, animerade illustrationer
- [DrawKit.com](https://www.drawkit.com/) - Gratis och premium illustrationer

### Färganpassning:
- Anpassa illustration till primärfärg (#6366F1) om möjligt
- Säkerställ god kontrast mot gradient-bakgrund

## Referens

- **Källdokument:** `docs/DESIGN_ANALYSIS_2025.md` sektion "Förslag 5: Förbättrad Inloggningssida"
- **Huvudissue:** `docs/issues/DESIGN_IMPLEMENTATION_SUB_ISSUES.md`
- **Relaterad dokumentation:** `docs/VISUAL_UX_IMPROVEMENTS.md`

## Estimerad Tidslinje

1. **Dag 1:** Skapa/hitta illustration, implementera split-screen layout
2. **Dag 2:** Uppdatera formulär-design och styling, lägg till fokus-animationer
3. **Dag 3:** Testa och finslipa (responsiv design, dark mode, animationer)

---

**Senast uppdaterad:** 2025-12-06  
**Version:** 1.0  
**Status:** Redo för implementation
