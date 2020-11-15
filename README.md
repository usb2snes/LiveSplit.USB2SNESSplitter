# LiveSplit.USB2SNESSplitter

Livesplit autosplit component using the Usb2Snes websocket protocol, see at [Usb2Snes](https://www.usb2snes.com)

Just copy the DLL in the Component folder of your Livesplit folder.

Run the Usb2Snes application (QUsb2Snes recommanded)

You need to add the component to your Livesplit layout, don't activate the autosplitter in the splits configuration. Then go into the component settings to select a device and a setting file, and to specify which split condition to use for each segment.

Look at the provided Super Metroid.json or alttp.json setting file to see how a config file looks.

If you need multiple address to validate a split you can use the `more` keyword
in the split definition

```json
{
      "name": "Morphing Ball",
      "address": "0xd873",
      "value": "0x04",
      "type": "bit",
      "more" : [
      	{
	      "address" : "0x4200",
	      "value" : "0x42",
	      "type" : "eq"
      	}
      ]
},
```

If you need a sequence of address/value to validate a split (like for alttp sword up) you can use the `next` keyword.

```json
{
      "name": "Tower of Hera",
      "address": "0xA0",
      "value": "0x07",
      "type": "eq",
      "next": [
        {
          "address": "0x10",
          "value": "0x13",
          "type": "eq"
        },
        {
          "address": "0x12E",
          "value": "0x2c",
          "type": "eq"
        }
      ]
},
```

To compile this it needs to be included into the full Livesplit source in the Component folder.

### Deprecated

Originally, your split names either had to match the names in the settings file, or you had to define aliases to use your own names. This is no longer necessary, because you can select whichever split condition you want for each segment in the component settings. However, aliases are still supported in the settings file. When a new split is added, if its name matches either a name or alias from the settings file, it will be automatically associated with that split condition.

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
