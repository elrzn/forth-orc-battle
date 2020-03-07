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

: player-dead? ( -- f ) player-health @ 0 <= ;
: monsters-dead? true ;

: player-attacks-per-round ( -- n )
  player-agility @ 15 / 1 + ;

: show-player ;
: show-monsters ;
: player-attack ;

: game-loop
  recursive
  player-dead? monsters-dead? or if
    exit
  then
  show-player
  player-attacks-per-round -1 do
    monsters-dead? not if
      show-monsters
      player-attack
    then
  loop
  ( TODO Make monsters attack. )
  game-loop
;

: .player-dead
  player-dead? if
    cr ." You have been killed. Game over." cr
  then ;

: .monsters-dead
  monsters-dead? if
    cr ." Congratulations! You have vanquised all of your foes." cr
  then ;

: orc-battle ( -- )
  init-monsters
  init-player
  game-loop
  .player-dead
  .monsters-dead ;
