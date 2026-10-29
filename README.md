# sts2-mod

**Spire Scaler** — Slay the Spire 2 用の「敵ステータス倍率 MOD」(敵の HP と攻撃力の倍率を変える難易度調整 MOD)。

- 仕様書: [docs/sts2-enemy-multiplier-mod-spec.md](docs/sts2-enemy-multiplier-mod-spec.md)

## 現在の状態: v0.6.0

敵に HP 倍率・攻撃力倍率を掛ける。幕(Act)ごと・敵の種類(通常敵 / エリート / ボス)ごとにも設定できる。倍率はゲーム内の MOD 設定画面(BaseLib)で変える。動作確認対象: ゲーム v0.111.0。

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

ゲーム内の MOD 設定 → Spire Scaler。表示はゲームの言語設定に合わせて日本語・English・简体中文・한국어 に切り替わる(それ以外の言語では English)。

- 敵の HP 倍率 / 攻撃力倍率(全体): 全ての幕・全ての敵に使う基本の倍率
- 第1〜3幕: 「第N幕を個別に設定する」をオンにすると、その幕では全体の倍率の代わりに、通常敵 / エリート / ボスごとの HP・攻撃力倍率を使う。オンにした時点の全体の倍率が初期値になる。第4幕以降(MOD で追加された幕など)は第3幕の設定を使う
- 敵の種類は戦闘(エンカウント)の種類で決まる。エリート戦・ボス戦で召喚された敵も、その戦闘の種類の倍率になる
- 各倍率はスライダーと数値入力のどちらでも変えられる(0.1 刻み)
- 詳細設定: オフのとき範囲は 1〜10 倍、オンで 0〜100 倍。オフに戻すと全倍率が 1.0 倍に戻る
- デフォルトに戻す: 全倍率を 1.0 倍に戻す(詳細設定と幕ごとのオン/オフはそのまま)
- 戦闘中に倍率を表示する: オンのとき、敵それぞれの体力バーのすぐ上に小さく「HP×○ / 攻撃×○」(その戦闘に掛かっている倍率)を表示する
- プリセット: スロット 1〜5。名前を入れて「保存」で今の設定(詳細設定・幕ごとのオン/オフ・全倍率)を保存、「読み込み」で切り替える。同じスロットに保存すると上書き。保存・読み込みのあと、スロットの下に結果を数秒表示する。保存先は `user://SpireScaler/presets.json`(Windows では `%APPDATA%\SlayTheSpire2\SpireScaler\presets.json`)

変更は次の戦闘から反映される。保存先は `user://mod_configs/SpireScaler.cfg`(Windows では `%APPDATA%\SlayTheSpire2\mod_configs\SpireScaler.cfg`)。
適用結果はゲームログ(`%APPDATA%\SlayTheSpire2\logs\godot.log`)に `[SpireScaler]` で出力される。

## マルチプレイ

- 全員が同じバージョンの Spire Scaler(と BaseLib)を入れている必要がある。入れていない人・バージョンが違う人がいると、ゲーム本体の照合で参加できず、足りない MOD 名がポップアップに出る
- 使う倍率はホストの設定。ホストがランを始めた時点の設定で固定され、参加者に送られる(参加者自身の設定は使わない)。ラン中に設定を変えても、そのランには反映されない
- ホストは固定した設定を `user://SpireScaler/multiplayer_run.json` に保存するので、保存したマルチプレイのランを続きから遊んでも同じ値になる
- シングルプレイは従来どおり、変更が次の戦闘から反映される
