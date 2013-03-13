PassphraseGenerator
===================

Simple and reusable C# code to generate passphrases based on a word dictionary and a configurable passphrase policy.

The whole system is meant to be wrapped up in as few code files as possible for easy integration into other programs.

The main issue to watch out for is the location of the embedded resource `diceware.txt` since embedded resource names are
automatically generated based on the default namespace setting of the project and containing folder names. Unfortunately,
there's no easy way for the code itself to "know" or "discover" the name of this resource, so modification of the static
constructor of the `PassphraseGenerator` class may be necessary.

Code Example
============

The main `Program.cs` serves as a good example of how to use the PassphraseGenerator class. It is extremely simple and
boils down to a simple static method call to generate a passphrase.

    for (int i = 0; i < 100; ++i)
    {
        string pp = PassphraseGenerator.Generate(PassphrasePolicy.Default);
        Console.WriteLine(pp);
    }

Examples of Generated Passphrases
=================================

Here's a list of example passphrases generated with the default policy:

    RimyCarneCueIdleLeery5
    MwMidstShadFjordHt1
    CodeRaterCometMig4
    LeviCharRaeBib100th2
    MaimCaneTrussMotet3
    TastyPiusRoamHop4
    SandHemMiniOdinXq0
    SadMotifTrashLj2
    BicepJuDunkFifo1
    KarpRifleLoafJump0
    MiltQedPyritePn3
    RevJulyCozyBach4
    ViceWastFussyGqWhig6
    LooseSimaGoutFdaTunis7
    PRunicDunhamYi5
    BelleFreyaMuralAtone2
    Cow69BureauLm5
    TiogaBoaRustHsGlenn3
    RpNihFourWhee2
    FmcGuideRiseJossAlgae6
    ModalDramGluedSolve4
    WhooshGaurWriteJones6
    VzTaffyMustTurk4
    PriorTyNepalHunk7
    CraigPyJabTaunt0
    TuneWhitAukLint1
    LimeDung2020CableQlPw8
    AxialRob29GlueyBotch7
    DelveTraitWeakByte2
    BrassyPaxSyriaGar1
    Drier6480LuxeRood8
    AxDavitRallyFarley8
    SoonGlomHonkFelt1
    200ChartAlienDream9
    ConicChinaSubtlyBarn0
    BeckScalpScopeGauss0
    BingNotchGummyCr9
    TrClicheKyOmega5
    StileRaoulMoCurbKi5
    AeIronGmAlai9
    HotboxAlpsLotOcean2
    TbLloydPithySavoy4
    KcCheapChipAli0
    ChideDewarIzClaus2
    $$KyotoAbaBrynMi4
    Here;TodayNbDish3
    CorbelAliceFlopTeem3
    DracoBuzzCrawlAile8
    RatLabAlaiDacca7
    BatheCeilGlTnSwirl7
    KapokRugZxPerCrazy3
    RitzDepthOlinLoftyPv1
    FactoDuke1500Slog0
    6000OatBookyFlint1
    PanelNoUzWl2
    IccHelenEaGuanoSlain5
    CarobRiyadhWeeWylieVv6
    SonnyDouseMayhemC1
    JiGinoGaffeDrama8
    HiveCasketGzLithe2
    GasArcHeapSatinQn3
    WhoaStokeNanLeyden4
    HoardRobBoyleFagRich7
    NycAbbotLuisQuark7
    FinnyAddleHeronOcKeel4
    KolaOyMonkIanOl6
    MonteFrillJuntaAbsorb9
    SonnyRingHotEll7
    SaltFaust52ndHokan0
    CmSoakGailGutTaftCn9
    HurtWhyClaraMajor6
    MdAngolaWitMotetPuma6
    KevinTsar51stLand3
    BlockBunyanVtStick9
    CtPunMalayButSuny2
    CmAmosReckKqNat7
    GammaStraw400Crony3
    84GlutAyeOtherNora4
    CulpaSell35CubGaur7
    DipSnakeAtlasMy0
    OnsetLeedsEarlRock4
    AmuseSetSackFb0
    94thHgUsurpFurl3
    AmFjBiotaTheir555
    DeemSeamyLipSuch0
    EdtDemitWlInchJoan3
    HipYcFileEast5
    BuGabonNetKnapp4
    StarkKoBunkEditor3
    ExertLastRailTide5
    GermRoachBassoHan6
    CrowdInnXrSpoilBarb5
    SatinDroveNasaJulep3
    BeepWashGogoRecur3
    RaoulBerryLyricRule3
    SwayDiInletMeyers6
    SloopTipoffSkShari9
    PomonaGurgleIkFebQt3
    ChalkMooreOmegaFauna7
    EnosChardChefVexXy1
