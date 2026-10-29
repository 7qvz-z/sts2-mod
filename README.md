# sts2-mod

**Spire Scaler** — Slay the Spire 2 用の「敵ステータス倍率 MOD」(敵の HP と攻撃力の倍率を変える難易度調整 MOD)。

- 仕様書: [docs/sts2-enemy-multiplier-mod-spec.md](docs/sts2-enemy-multiplier-mod-spec.md)

## 現在の状態: v0.2.0

全ての敵に HP 倍率・攻撃力倍率を掛ける。倍率はゲーム内の MOD 設定画面(BaseLib)で変える。動作確認対象: ゲーム v0.111.0。

前提 MOD: [BaseLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127) 3.4.7 以上。

## ビルドとインストール

.NET 9 SDK 以上が必要。

```
dotnet build SpireScaler -c Release
```

ビルド後、`<ゲームフォルダ>\mods\SpireScaler\` に `SpireScaler.dll` と `SpireScaler.json` がコピーされる。
ゲームフォルダが既定と違う場合は `-p:GameDir="D:\...\Slay the Spire 2"`、コピーしない場合は `-p:InstallToGame=false` を付ける。
BaseLib はワークショップの購読先を参照する。別の場所にある場合は `-p:BaseLibDll="...\BaseLib.dll"` を付ける。

## 設定

ゲーム内の MOD 設定 → Spire Scaler。

- 敵の HP 倍率 / 攻撃力倍率: スライダーと数値入力のどちらでも変えられる(0.01 刻み)
- 詳細設定: オフのとき範囲は 1〜10 倍、オンで 0〜100 倍。オフに戻すと全倍率が 1.0 倍に戻る
- デフォルトに戻す: 全倍率を 1.0 倍に戻す(詳細設定のオン/オフはそのまま)

変更は次の戦闘から反映される。保存先は `user://mod_configs/SpireScaler.cfg`(Windows では `%APPDATA%\SlayTheSpire2\mod_configs\SpireScaler.cfg`)。
適用結果はゲームログ(`%APPDATA%\SlayTheSpire2\logs\godot.log`)に `[SpireScaler]` で出力される。
