//Original Scripts by IIColour (IIColour_Spectrum)

using System.Runtime.CompilerServices;

[System.Serializable]
public class PC
{
    public Pokemon[][] boxes = new Pokemon[][]
    {
        new Pokemon[6], //Party
        new Pokemon[30], //Boxes
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
        new Pokemon[30],
    };

    public int[] boxTexture = new int[13];

    public PC() { }

    /**
     *判断是否有剩余空位置
     *@param index 索引号
     */
    private int hasSpace(int index)
    {
        for (int i = 0; i < boxes[index].Length; i++)
        {
            if (boxes[index][i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    /**
     *获取盒子精灵数量
     *@param index 索引号
     */
    [MethodImpl(MethodImplOptions.Synchronized)]
    public int getBoxPokemonLength(int index)
    {
        int result = 0;
        for (int i = 0; i < boxes[index].Length; i++)
        {
            if (boxes[index][i] != null)
            {
                result += 1;
            }
        }
        return result;
    }

    /**
     *整理箱子
     */
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void pack(int index)
    {
        int boxLength = boxes[index].Length;
        Pokemon[] packedArray = new Pokemon[boxLength];
        int i2 = 0;
        for (int i = 0; i < boxLength; i++)
        {
            if (boxes[index][i] != null)
            {
                packedArray[i2] = boxes[index][i];
                i2 += 1;
            }
        }
        boxes[index] = packedArray;
    }

    /**
    *增加一个精灵
    *@param acquiredPokemon
    */
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool addPokemon(Pokemon acquiredPokemon, int first)
    {
        for (int i = first; i < boxes.Length; i++)
        {
            int space = hasSpace(i);
            if (space != -1)
            {
                boxes[i][space] = acquiredPokemon;
                return true;
            }
        }
        return false;
    }

    /**
    *移除对应的精灵
    */
    public bool removePokemon(Pokemon acquiredPokemon)
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            int length = boxes[i].Length;
            for (int j = 0; j < length; j++)
            {
                if (boxes[i][j] == acquiredPokemon)
                {
                    boxes[i][j] = null;
                    return true;
                }
            }
        }
        return false;
    }


    /**
    *交换一个精灵位置
    */
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void swapPokemon(int box1, int pos1, int box2, int pos2)
    {
        Pokemon temp = boxes[box1][pos1];
        boxes[box1][pos1] = boxes[box2][pos2];
        boxes[box2][pos2] = temp;
    }
}