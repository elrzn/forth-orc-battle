30 constant player-attr-default

variable player-health
variable player-agility
variable player-strength

: (player-decrease)        ( n addr -- ) dup @ rot - swap ! ;
: player-decrease-health   ( n -- )      player-health  (player-decrease) ;
: player-decrease-agility  ( n -- )      player-agility (player-decrease) ;
: player-decrease-strength ( n -- )      player-health  (player-decrease) ;

12 constant #monsters
create monsters #monsters cells allot

: randval ( n -- n' ) choose 1+ ;
: n-input ( -- n )    pad 5 blank pad 5 accept >r 0. pad r> >number 2drop drop ;

struct
  dcell% field monster-name
  cell%  field monster-health
  \ Allocate addresses that point to method implementations that may be
  \ overriden. Another option would have been a single cell holding a virtual
  \ table but for the amount of methods I'd rather not create another level of
  \ indirection.
  cell%  field monster-show-addr
  cell%  field monster-attack-addr
  cell%  field monster-hit-addr
end-struct monster%

: monster-name!  ( c-addr u addr -- ) monster-name 2! ;
: monster-name@  ( addr -- c-addr u ) monster-name 2@ ;
: monster-dead?  ( addr -- f )        monster-health @ 0<= ;
: -monster-dead? ( addr -- f )        monster-dead? not ;
: .monster-name  ( addr -- )          monster-name@ type ;

: monster-take-damage ( addr n -- )
  over monster-health @
  swap -
  swap monster-health ! ;

: (monster-hit) ( addr n -- )
  2dup monster-take-damage
  over monster-dead? cr if
    drop ." You killed the " .monster-name ." !"
  else
    swap ." You hit the " .monster-name ." , knocking off " . ." health points!"
  then ;

: monster-show           ( addr -- )   dup  monster-show-addr   @ execute ;
: monster-attack         ( addr -- )   dup  monster-attack-addr @ execute ;
: monster-hit            ( addr n -- ) over monster-hit-addr    @ execute ;
: .monster               ( addr -- )   ." A fierce " .monster-name ;
: monster-default-health ( addr -- )   10 randval swap monster-health ! ;
: monster-default-name   ( addr -- )   s" Monster" rot monster-name! ;
: monster-default-show   ( addr -- )   ['] .monster swap monster-show-addr ! ;
: monster-default-hit    ( addr -- )   ['] (monster-hit) swap monster-hit-addr ! ;

: make-monster-defaults ( addr -- addr )
  dup monster-default-health
  dup monster-default-name
  dup monster-default-show
  dup monster-default-hit ;

: make-monster ( -- addr ) monster% %allot make-monster-defaults ;

: monsters-attack
  #monsters 0 do
    monsters i cells + @
    dup monster-dead? if
      drop
    else
      monster-attack cr
    then
  loop ;

monster%
  cell% field wicked-orc-club-level
end-struct wicked-orc%

: .wicked-orc ( addr -- )
  ." A wicked orc with a level " wicked-orc-club-level ? ." club" ;

: wicked-orc-attack ( addr -- )
  wicked-orc-club-level @ randval
  ." And orc swings his club at you and knocks off "
  dup . ." of your health points"
  player-decrease-health ;

: make-wicked-orc ( -- addr )
  wicked-orc% %allot make-monster-defaults
  dup s" Wicked Orc" rot monster-name!
  dup ['] .wicked-orc       swap monster-show-addr   !
  dup ['] wicked-orc-attack swap monster-attack-addr !
  8 randval over wicked-orc-club-level ! ;

monster% end-struct hydra%

: .hydra ( addr -- ) ." A malicious hydra with " monster-health ? ." heads" ;

: hydra-increase-health ( addr -- ) monster-health @ 1 + swap monster-health ! ;

: hydra-attack ( addr -- )
  dup monster-health @ 1 - randval
  ." A hydra attacks you with " dup . ." of its heads! It also grows back one more head!"
  swap dup hydra-increase-health
  player-decrease-health ;

: hydra-hit ( addr n -- )
  tuck monster-take-damage
  over monster-dead? cr if
    drop
    ." The corpse of the fully decapitated and decapacitated hydra falls to the floor!"
  else
    ." You lop off " . ." of the hydra's heads!"
  then ;

: make-hydra ( -- addr )
  hydra% %allot make-monster-defaults
  dup s" Hydra" rot monster-name!
  dup ['] .hydra       swap monster-show-addr   !
  dup ['] hydra-attack swap monster-attack-addr ! ;

monster%
  cell% field slime-mold-sliminess
end-struct slime-mold%

: .slime-mold ( addr -- )
  ." A slime mold with a sliminess of " slime-mold-sliminess ? ;

: slime-mold-attack ( addr -- )
  slime-mold-sliminess @ randval
  ." A slime mold wraps around your legs and decreases your agility by " dup . ." !"
  2 choose 0= if
    ."  It also squirts in your face, taking away a health point!"
    player-decrease-health
  then ;

: make-slime-mold ( -- addr )
  slime-mold% %allot make-monster-defaults
  5 randval over slime-mold-sliminess !
  dup s" Slime mold" rot monster-name!
  dup ['] .slime-mold       swap monster-show-addr   !
  dup ['] slime-mold-attack swap monster-attack-addr ! ;

monster% end-struct brigand%

: brigand-attack ( addr -- )
  drop           \ do nothing, but keep monster-attack interface consistent
  player-health player-agility player-strength max max ( n )
  case
    player-health of
      ." A brigand hits you with his slingshot, taking off 2 health points!"
      2 player-decrease-health
    endof
    player-agility of
      ." A brigand catches your leg with his whip, taking off 2 agility points!"
      2 player-decrease-agility
    endof
    player-strength of
      ." A brigand cuts your arm with his whip, taking off 2 strength points!"
      2 player-decrease-strength
    endof
  endcase ;

: make-brigand ( -- addr )
  brigand% %allot make-monster-defaults
  dup s" Brigand" rot monster-name!
  dup ['] brigand-attack swap monster-attack-addr ! ;

\ Keep track of builders to aid random creation of monsters.
4 constant #monsters-builders
create monster-builders ' make-wicked-orc ,
                        ' make-hydra      ,
                        ' make-slime-mold ,
                        ' make-brigand    ,

: init-player
  player-attr-default player-health   !
  player-attr-default player-agility  !
  player-attr-default player-strength ! ;

: player-dead? ( -- f ) player-health @ 0<= ;

: player-attacks-per-round ( -- n ) player-agility @ 15 / 1+ ;

: .player
  ." You are a valiant knight with a health of "
  \ Note that ? or @ . is printing a space afterwards. Need to figure out how to
  \ circunvent this.
  player-health ?
  ." , an agility of "
  player-agility ?
  ." , and a strength of "
  player-strength ?
  ." ." ;

: pick-monster ( -- addr )
  recursive
  cr ." Monster #: "
  monsters n-input 1 - cells + @
  dup monster-dead? if
    cr ." That monster is already dead"
    drop pick-monster
  then ;

: random-monster ( -- addr )
  recursive
  monsters #monsters choose cells + @
  dup monster-dead? if
    drop random-monster
  then ;

: make-random-monster ( -- addr ) monster-builders #monsters-builders choose cells + @ execute ;

: init-monsters
  #monsters 0 do
    make-random-monster monsters i cells + !
  loop ;

: show-monsters
  cr ." Your foes: "
  #monsters 0 do
    cr i 1 + . ." -> "
    monsters i cells + @
    dup monster-dead? if
      drop ." **dead**"
    else
      ." Health = " dup monster-health ? ." " monster-show
    then
  loop ;

: monsters-dead? ( -- f )
  #monsters 0 do
    monsters i cells + @ -monster-dead? if
      false leave
    then
  loop
  0= not ;

: monsters-alive? monsters-dead? not ;

: player-stab-attack
  pick-monster                    ( addr )
  player-strength @ 2/ randval 2+ ( addr n )
  monster-hit ;

: player-double-swing-attack
  pick-monster                  ( addr )
  player-strength @ 6 / randval ( addr n )
  cr ." Your double swing has a strength of " dup .
  tuck                          ( n addr n )
  monster-hit                   ( n )
  monsters-alive? if
    pick-monster swap           ( addr n )
    monster-hit
  else
    drop
  then ;

: player-attack-other
  player-strength @ 3 / 1+ -1 do
    monsters-alive? if
      random-monster 1 monster-hit
    then
  loop ;

: player-attack
  cr ." Attack style: [s]tab [d]ouble swing [r]oundhouse:"
  key case
    's' of player-stab-attack         endof
    'd' of player-double-swing-attack endof
           player-attack-other
  endcase ;

: game-loop
  recursive
  player-dead? monsters-dead? or if
    exit
  then
  .player
  player-attacks-per-round -1 do
    monsters-alive? if
      show-monsters
      player-attack
    then
  loop
  monsters-attack
  game-loop ;

: .player-dead
  player-dead? if
    ." You have been killed. Game over." cr
  then ;

: .monsters-dead
  monsters-dead? if
    cr ." Congratulations! You have vanquised all of your foes." cr
  then ;

: end-game   .player-dead .monsters-dead  ;
: init-game  cr init-monsters init-player ;
: orc-battle init-game game-loop end-game ;
