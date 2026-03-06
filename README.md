# 🔗 GameBindingSystem

**Unity için reaktif state yönetim framework'ü**

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity)](https://unity.com)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![C#](https://img.shields.io/badge/C%23-9.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)

GameBindingSystem, Unity projelerinde **game state** yönetimini ve bu state'in **UI** ile **gameplay** bileşenlerine otomatik senkronizasyonunu sağlayan bir reaktif framework'tür. Backend-agnostik tasarımı sayesinde **ScriptableObject**, **SQLite**, **JSON**, **REST API** veya herhangi bir veri kaynağı ile kullanılabilir.

```csharp
// Önce: 20+ satır property setter, UpdateVisuals() çağrıları...
// Sonra:
public Bindable<float> Health = new(100f);
public Bindable<float> MaxHealth = new(100f);

void Start()
{
    healthSlider.BindValue(Health, MaxHealth, (h, m) => h / m);
    healthText.BindText(Health, MaxHealth, (h, m) => $"HP: {h:0}/{m:0}");
    healthBar.BindColor(Health, h => h < 30 ? Color.red : Color.green);
}
// Health.Value değiştiğinde tüm UI otomatik güncellenir!
```

---

## ✨ Özellikler

| Özellik | Açıklama |
|---|---|
| 🎯 **Reaktif State** | `Bindable<T>` ile değer değişimlerini otomatik yay |
| 🔄 **İki Yönlü Binding** | Slider ↔ Data ↔ Label senkronizasyonu |
| 📦 **Reaktif Koleksiyonlar** | `BindableList<T>`, `BindableDictionary<K,V>` eleman-düzeyinde olaylarla |
| 🧮 **Türetilmiş State** | `Derived.From()` ile birden fazla kaynağı birleştir |
| 🎬 **Animasyon** | `BindableAnimator` + 30 easing curve (easings.net) |
| 🛡️ **Thread-Safe** | Farklı thread'den güvenli değer ataması |
| ⚡ **UniTask Opsiyonel** | Yüklüyse otomatik algılanır, yoksa sorunsuz çalışır |
| 🗄️ **Backend Agnostik** | SO, SQLite, JSON, REST — veri kaynağından bağımsız |
| 🔧 **Inspector Desteği** | `Bindable<T>` alanları editör'de doğrudan düzenlenebilir |
| ♻️ **Otomatik Temizlik** | `BindContext` ile binding'ler destroy'da otomatik kaldırılır |

---

## 📁 Proje Yapısı

```
Scripts/GameBindingSystem/
├── Runtime/
│   ├── Core/
│   │   ├── Bindable.cs              # Bindable<T>, IBindable<T>
│   │   ├── BindContext.cs            # Binding yaşam döngüsü
│   │   ├── BindRunner.cs            # Singleton dispatcher
│   │   └── Derived.cs               # Türetilmiş state
│   ├── Collections/
│   │   ├── BindableList.cs           # Reaktif liste
│   │   └── BindableDictionary.cs     # Reaktif sözlük
│   ├── Animation/
│   │   ├── Curve.cs                  # 30 easing + Bezier + Custom
│   │   └── BindableAnimator.cs       # Float animatör
│   └── Extensions/
│       ├── BindableExtensions.cs     # Bind, BindUpdate, BindInterval
│       ├── BindableAsyncExtensions.cs # UniTask async (opsiyonel)
│       ├── UGUIBindingExtensions.cs  # TMP, Image, Slider, Toggle...
│       └── TransformExtensions.cs    # Position, Rotation, Scale
├── Editor/
│   └── BindablePropertyDrawer.cs     # Inspector desteği
├── Samples/                          # 7 zengin örnek
└── GameBindingSystem.asmdef
```

---

## 🚀 Kurulum

### Unity Package olarak (Git URL)
1. Unity'de **Window → Package Manager** açın
2. **+** butonuna basın → **Add package from git URL**
3. Repo URL'sini yapıştırın

### Manuel
1. Bu repoyu `Assets/Scripts/GameBindingSystem/` altına kopyalayın
2. Unity'nin otomatik derleme yapmasını bekleyin

### Gereksinimler
- Unity **2021.3** veya üzeri
- **TextMeshPro** (Unity'de varsayılan olarak gelir)
- **UniTask** *(opsiyonel — yüklüyse otomatik algılanır)*

---

## 📖 Kullanım Kılavuzu

### 1. Bindable Tanımlama

```csharp
using BindingSystem;

public class Player : MonoBehaviour
{
    // Inspector'da düzenlenebilir reaktif alanlar
    public Bindable<string> PlayerName = new("Hero");
    public Bindable<int> Health = new(100);
    public Bindable<float> Speed = new(5.0f);
    public Bindable<Sprite> Portrait;
    
    // Read-only erişim için IBindable
    public IBindable<int> ReadOnlyHealth => Health;
}
```

### 2. UI'a Bağlama

```csharp
void Start()
{
    // Direkt binding
    nameText.BindText(player.PlayerName);
    portrait.BindSprite(player.Portrait);
    
    // Transform ile binding
    healthText.BindText(player.Health, h => $"HP: {h}");
    
    // Çoklu kaynak
    statusText.BindText(player.Health, player.PlayerName,
        (h, name) => $"{name}: {h} HP");
    
    // Slider (iki yönlü)
    volumeSlider.BindValueTwoWay(settings.Volume);
}
```

### 3. Türetilmiş State (Derived)

```csharp
// Tek kaynak
var healthPercent = Derived.From(Health, MaxHealth,
    (h, m) => m > 0 ? h / m : 0f);

// veya kısayol
var displayName = PlayerName.Derive(n => n.ToUpper());

// İç bindable çıkarma
var playerHealth = CurrentPlayer.Derive(p => p.Health);
```

### 4. Reaktif Liste

```csharp
public BindableList<Item> Inventory = new();

void Start()
{
    Inventory.OnElementAdd += args =>
        InstantiatePrefab(args.Value, args.Index);
    
    Inventory.OnElementRemove += args =>
        DestroySlot(args.Index);
}

// Türetilmiş liste
var itemNames = Inventory.DeriveSelect(item => item.Name);
```

### 5. Animasyon

```csharp
var anim = new BindableAnimator(Curve.EaseOutCubic(1f), autoPlay: true);

// Smooth pozisyon değişimi
transform.BindPosition(anim, t => Vector3.Lerp(start, end, t));

// Smooth health bar
slider.BindValue(anim, t => Mathf.Lerp(oldHealth, newHealth, t));

// Tamamlanmayı bekle
await anim.WaitForCompleteAsync();
```

### 6. ScriptableObject ile

```csharp
[CreateAssetMenu(menuName = "Game/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    public Bindable<string> Name = new("Hero");
    public Bindable<int> Level = new(1);
    public Bindable<float> Health = new(100f);
}

// UI script'inde:
void Start()
{
    nameText.BindText(characterSO.Name);
    levelText.BindText(characterSO.Level, lv => $"Lv. {lv}");
}
// SO'daki değeri değiştirince tüm bağlı UI'lar güncellenir
```

### 7. SQLite ile

```csharp
public Bindable<int> Gold = new(0);

void Start()
{
    // Yükle
    Gold.Value = db.ExecuteScalar<int>("SELECT Gold FROM Player");
    
    // Dirty flag ile otomatik kaydet
    Gold.OnChanged += () => saveManager.MarkDirty("Gold");
    
    this.BindInterval(5f, () => {
        if (saveManager.IsDirty) SaveToDatabase();
    });
}
```

### 8. BindContext (Yaşam Döngüsü)

```csharp
// Varsayılan: component destroy olunca otomatik unbind
healthText.BindText(Health); // HealthText destroy = binding temizlenir

// Manuel kontrol: OnEnable/OnDisable
private BindContext _ctx;
void OnEnable()
{
    _ctx = new BindContext();
    _ctx.Bind(Health, h => healthText.text = $"HP: {h}");
}
void OnDisable() => _ctx.Unbind();

// CancellationToken ile
var ctx = new BindContext(destroyCancellationToken);
```

---

## 🎬 Mevcut Easing Curve'ler

`Curve` sınıfı [easings.net](https://easings.net/) tabanlı **30 hazır easing** sunar:

| Grup | In | Out | InOut |
|---|---|---|---|
| **Quad** | `EaseInQuad` | `EaseOutQuad` | `EaseInOutQuad` |
| **Cubic** | `EaseInCubic` | `EaseOutCubic` | `EaseInOutCubic` |
| **Quart** | `EaseInQuart` | `EaseOutQuart` | `EaseInOutQuart` |
| **Quint** | `EaseInQuint` | `EaseOutQuint` | `EaseInOutQuint` |
| **Sine** | `EaseInSine` | `EaseOutSine` | `EaseInOutSine` |
| **Expo** | `EaseInExpo` | `EaseOutExpo` | `EaseInOutExpo` |
| **Circ** | `EaseInCirc` | `EaseOutCirc` | `EaseInOutCirc` |
| **Back** | `EaseInBack` | `EaseOutBack` | `EaseInOutBack` |
| **Elastic** | `EaseInElastic` | `EaseOutElastic` | `EaseInOutElastic` |
| **Bounce** | `EaseInBounce` | `EaseOutBounce` | `EaseInOutBounce` |

Ek olarak: `Linear`, `Bezier(duration, x1, y1, x2, y2)`, `new Curve(AnimationCurve)`, `new Curve(Func<float,float>, duration)`

---

## 🧩 UGUI Extension Method'ları

| Method | Hedef Component | Açıklama |
|---|---|---|
| `BindText` | `TMP_Text` | 1-3 kaynaklı text binding |
| `BindColor` | `Graphic` / `Image` / `TMP_Text` | Renk binding |
| `BindSprite` | `Image` | Sprite binding |
| `BindFillAmount` | `Image` | Fill amount (1-2 kaynak) |
| `BindValue` | `Slider` | Value binding (1-2 kaynak) |
| `BindValueTwoWay` | `Slider` | İki yönlü slider ↔ data |
| `BindIsOn` / `TwoWay` | `Toggle` | Bool binding |
| `BindValue` / `TwoWay` | `TMP_Dropdown` | Dropdown seçimi |
| `BindText` / `TwoWay` | `TMP_InputField` | Text input binding |
| `BindInteractable` | `Selectable` | Etkileşim durumu |
| `BindAlpha` | `CanvasGroup` | Alpha binding |
| `BindActive` | `CanvasGroup` / `Component` | Aktiflik binding |
| `BindClick` | `Button` | Click event binding |
| `BindPosition` | `Transform` | World/Local pozisyon |
| `BindRotation` | `Transform` | Rotation binding |
| `BindLocalScale` | `Transform` | Scale binding |

---

## ⚡ UniTask Desteği

UniTask **opsiyoneldir**. Yüklüyse `GameBindingSystem.asmdef` içindeki `versionDefines` ile otomatik algılanır.

```csharp
#if UNITASK_ENABLED
// Değer değişene kadar bekle
var newHealth = await Health.WaitForChangeAsync(cancellationToken);

// Koşul sağlanana kadar bekle
await Gold.WaitUntilAsync(g => g >= 100, cancellationToken);

// Animasyon bitişini bekle
await animator.ToUniTask(cancellationToken);
#endif
```

UniTask yoksa → `BindableAnimator.WaitForCompleteAsync()` standart `Task` döndürür. Framework %100 çalışır.

---

## ⚠️ Dikkat Edilmesi Gerekenler

### Thread Safety
`Bindable<T>.Value` setter'ı thread-safe'dir. Farklı thread'den (async SQLite, network vb.) değer atandığında otomatik olarak main thread'e yönlendirilir.

### Circular Binding Koruması
İki yönlü binding'lerde `EqualityComparer<T>.Default` ile sonsuz döngü önlenir — aynı değer tekrar atandığında event tetiklenmez.

### Bellek Yönetimi
- Varsayılan `BindContext` destroy'da otomatik temizlenir
- Manuel `BindContext` oluşturursanız **mutlaka** `Unbind()` çağırın
- `BindableList`/`BindableDictionary` event'lerini manuel unsubscribe etmeyi unutmayın

### SQLite Kaydetme Stratejisi
Her `OnChanged`'da kaydetmek yerine **dirty flag + batch save** desenini kullanın:
```csharp
this.BindInterval(5f, () => {
    if (saveManager.IsDirty) SaveToDatabase();
});
```

---

## 📂 Örnekler (Samples)

| # | Örnek | Açıklama |
|---|---|---|
| 01 | **Basic Binding** | Health bar: Text + Slider + Color binding |
| 02 | **List Binding** | `BindableList` + prefab instantiate/destroy |
| 03 | **ScriptableObject** | SO üzerinden paylaşılan karakter verisi |
| 04 | **SQLite Integration** | Dirty flag + batch save pattern |
| 05 | **Animated UI** | `BindableAnimator` ile smooth geçişler |
| 06 | **Two-Way Binding** | Ayarlar paneli — slider ↔ data ↔ label |
| 07 | **Derived Chain** | Shop: Gold → CanAfford → Button.interactable |
| 08 | **Form Validation** | Birden çok Derived birleşimi, Regex, mesajlar |
| 09 | **List Filtering** | `BindableList` üzerinde canlı arama/filtreleme |
| 10 | **Tab Navigation** | `Bindable<int>` ile CanvasGroup tab kontrolü |

Her örnek `Samples/` klasöründe bağımsız script'ler içerir. Unity'de sahne ve UI objeleri oluşturup script referanslarını bağlamanız yeterlidir.

---

## 📜 Lisans

MIT License — Detaylar için [LICENSE](LICENSE) dosyasına bakınız.