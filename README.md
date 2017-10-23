# DMXtable
C# WPF App for Enttec DMX USB PRO

## Serial format
While this source code might not be useful to anyone but myself, I'll leave the documentation on formatting a serial message to the 
Enttex DMX USB PRO here for posterity.

```
 https://www.enttec.com/docs/dmx_usb_pro_api_spec.pdf
  
 Application Message Format
 
                         HEX        DEC
 - Start of message      7E         126
 - Type of message
      Send DMX           06         06
 - Data Length LSB       Data Length & 255
 - Data Length MSB       (Data Length >> 8) & 255
 - Leading 0*            00         00
 - Data bytes
 - End of message        E7         231
 
 * Undocumented. Possibly universe no.?
 ```
 
 So a properly formatted message for 4 channels 255 would be:
 ```
 0x7E 0x06 0x04 0x00 0x00 0xFF 0xFF 0xFF 0xFF 0xE7
 ```
