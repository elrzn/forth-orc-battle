variable player-health
variable player-agility
variable player-strength

variable monsters
variable monster-builders

12 constant monster-num

: randval ( n -- n' ) choose 1+ ;

struct
  cell% field monster-health
end-struct monster%

: make-monster ( -- monster )
  monster% %allot
  10 randval over monster-health ! ;

: init-player
  30 player-health   !
  30 player-agility  !
  30 player-strength ! ;

: player-dead? ( -- f ) player-health @ 0 <= ;

: player-attacks-per-round ( -- n )
  player-agility @
  15 /                          \ no need for truncating integer operations
  1+ ;

: show-player
  cr
  ." You are a valiant knight with a health of "
  \ Note that ? or @ . is printing a space afterwards. Need to figure out how to
  \ circunvent this.
  player-health ?
  ." , an agility of "
  player-agility ?
  ." , and a strength of "
  player-strength ?
  ." ." ;

: monster-hit ( monster damage -- ) ;  \ nyi

: pick-monster   ( -- monster ) ;  \ nyi 178
: random-monster ( -- monster ) ;  \ nyi 177

: init-monsters ;               \ nyi 178
: show-monsters ;               \ nyi
: monsters-dead? true ;         \ nyi
: monsters-alive? monsters-dead? not ;

: player-stab-attack
  pick-monster                    ( monster )
  player-strength @ 2/ randval 2+ ( monster damage )
  monster-hit ;

: player-double-swing-attack
  pick-monster                  ( monster )
  player-strength @ 6 / randval ( monster damage )
  cr ." Your double swing has a strength of " dup .
  tuck                          ( damage monster damage )
  monster-hit                   ( damage )
  monsters-alive? if
    pick-monster swap           ( monster damage )
    monster-hit
  else
    drop
  then ;

: player-attack-other
  player-strength @ 3 / 1+      ( times )
  -1 do
    monsters-alive? if
      random-monster 1 monster-hit
    then
  loop ;

: player-attack
  cr ." Attack style: [s]tab [d]ouble swing [r]oundhouse:"
  key case
    115 of player-stab-attack         endof
    100 of player-double-swing-attack endof
           player-attack-other
  endcase ;

: game-loop
  recursive
  player-dead? monsters-dead? or if
    exit
  then
  show-player
  player-attacks-per-round -1 do
    monsters-alive? if
      show-monsters
      player-attack
    then
  loop
  \ TODO Make monsters attack. Basically need to loop through the array of
  \ monsters, and if the monster is not dead, then make it attack the player.
  game-loop ;

: .player-dead
  player-dead? if
    cr ." You have been killed. Game over." cr
  then ;

: .monsters-dead
  monsters-dead? if
    cr ." Congratulations! You have vanquised all of your foes." cr
  then ;

: end-game   .player-dead .monsters-dead  ;
: init-game  init-monsters init-player    ;
: orc-battle init-game game-loop end-game ;
