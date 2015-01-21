module LifeGameLogic

type Life = { x : int; y : int; surv : bool; }

let getX i width = i % width
let getY i height = i / height

(* 周囲の生き物の数 *)
let getSurroundedLifes (data : Life array) (life : Life) (width : int) (height : int) : int =
  seq { for y1 in [life.y - 1 .. life.y + 1] do
          for x1 in [life.x - 1 .. life.x + 1] do
            if x1 <> life.x || y1 <> life.y then
              let i = ((if y1 < 0 then y1 + height else y1) % height) * width + (if x1 < 0 then x1 + width else x1) % width
              if data.[i].surv then yield data.[i]}
  |> Seq.length
// 遅くてお話しにならない
//  data
//  |> Array.filter
//      (fun life1 -> life1 <> life && life1.surv &&
//                    abs (life1.x - life.x) <= 1 && abs (life1.y - life.y) <= 1)
//  |> Array.length

(* 指定された座標の生き物が生きるかどうか？ *)
let isSurvive (data : Life array) (life : Life) (width : int) (height : int) =
  let count = getSurroundedLifes data life width height
  if life.surv
  // 生存
  // 過疎・過密
  then count = 2 || count = 3
  // 誕生
  else count = 3

(* ライフゲームメインロジック *)
let logic (data : bool seq, width : int, height : int) =
  let data1 = data
              |> Seq.toArray
              // データに座標を付加する
              |> Array.mapi (fun i surv -> { x = getX i width; y = getY i height; surv = surv })
  data1
  // 生死を決める
  |> Array.map (fun life -> { life with surv = isSurvive data1 life width height })
  // データを元に戻す
  |> Array.map (fun life -> life.surv)
