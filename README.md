# LiveSplit.USB2SNESSplitter

Livesplit autosplit component using the Usb2Snes websocket protocol, see at [Usb2Snes](https://www.usb2snes.com)

Just copy the DLL in the Component folder of your Livesplit folder.

Run the Usb2Snes application

You need to add the component to your Livesplit layout, don't activate the autosplitter in the splits configuration. Then goes into the component settings to select a device and a setting file.

Look at the provided Super Metroid.json setting file to see how a config file look.

Your splits names much match the category definition in the setting file but you can define aliases (at the same level than the categories definitions) if you want to use your own name.

```json
"alias": {
    "Ze Bombs": "Bombs",
    "Kraid Slow Kill": "Varia Suit",
    "Wave for the camera": "Wave Beam",
    "Chozobuster": "Phantoon",
    "Space Jump Sucks": "Space Jump",
    "MB: My bad": "Mother Brain 3 Down (Escape Message)"
  },
```

To compile this it needs to be included into the full Livesplit source in the Component folder.

