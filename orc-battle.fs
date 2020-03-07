variable player-health
variable player-agility
variable player-strength

variable monsters
variable monster-builders

12 constant monster-num

: init-monsters ;

: init-player
  30 player-health   !
  30 player-agility  !
  30 player-strength ! ;

: game-loop ;
: player-dead? ( -- f ) player-health @ 0 <= ;
: monsters-dead? true ;

: orc-battle ( -- )
  init-monsters
  init-player
  game-loop
  player-dead? if
    cr ." You have been killed. Game over." cr
  then
  monsters-dead? if
    cr ." Congratulations! You have vanquised all of your foes." cr
  then ;
