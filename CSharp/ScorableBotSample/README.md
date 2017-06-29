# scorablebot
scorablebot

Invocation order

1. For all scorables bot framework does the following :
  - calls HasScore()
  - if HasScore() then GetScore()
2. Bot framework then Compare scorables and
3. Call PostAsync() on highest scorable (this is where to inject the dialog)
4. Call DoneAsync() on all scorables
