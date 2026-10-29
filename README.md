# sts2-mod

**Spire Scaler** — Slay the Spire 2 用の「敵ステータス倍率 MOD」(敵の HP と攻撃力の倍率を変える難易度調整 MOD)。

- 仕様書: [docs/sts2-enemy-multiplier-mod-spec.md](docs/sts2-enemy-multiplier-mod-spec.md)

## 現在の状態: 最小版 (v0.1.0)

全ての敵に HP 倍率・攻撃力倍率を掛ける。設定はファイル編集。動作確認対象: ゲーム v0.111.0。

## ビルドとインストール

.NET 9 SDK 以上が必要。

```
dotnet build SpireScaler -c Release
```

ビルド後、`<ゲームフォルダ>\mods\SpireScaler\` に `SpireScaler.dll` と `SpireScaler.json` がコピーされる。
ゲームフォルダが既定と違う場合は `-p:GameDir="D:\...\Slay the Spire 2"`、コピーしない場合は `-p:InstallToGame=false` を付ける。

## 設定

初回起動時に `user://SpireScaler/config.cfg`(Windows では `%APPDATA%\SlayTheSpire2\SpireScaler\config.cfg`)が作られる。

```
[multipliers]
hp=1.0
attack=1.0
```

範囲は 0〜100。変更は次の戦闘から反映される。適用結果はゲームログ(`%APPDATA%\SlayTheSpire2\logs\godot.log`)に `[SpireScaler]` で出力される。
