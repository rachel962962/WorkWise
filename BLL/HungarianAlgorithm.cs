using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class HungarianAlgorithm
    {
        public static int[] FindAssignments(this double[,] costs)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            var h = costs.GetLength(0);
            var w = costs.GetLength(1);

            // טיפול במקרה שבו יש יותר שורות מעמודות
            bool rowsGreaterThanCols = h > w;
            if (rowsGreaterThanCols)
            {
                // טרנספוזיציה של מטריצת העלויות כדי להבטיח שיש יותר עמודות משורות
                // זה מפשט את מימוש האלגוריתם
                var row = w;
                var col = h;
                var transposeCosts = new double[row, col];
                for (var i = 0; i < row; i++)
                {
                    for (var j = 0; j < col; j++)
                    {
                        transposeCosts[i, j] = costs[j, i];
                    }
                }
                costs = transposeCosts;
                h = row;
                w = col;
            }

            // שלב 0: הפחתת השורות על ידי חיסור הערך המינימלי בכל שורה מכל האיברים באותה שורה
            // פעולה זו יוצרת לפחות אפס אחד בכל שורה, מה שנחוץ למציאת שיבוצים תקפים
            for (var i = 0; i < h; i++)
            {
                var min = double.MaxValue;

                for (var j = 0; j < w; j++)
                {
                    min = Math.Min(min, costs[i, j]);
                }

                for (var j = 0; j < w; j++)
                {
                    costs[i, j] -= min;
                }
            }

            // אתחול מטריצת מסיכות:
            // 0 = לא מסומן, 1 = אפס מסומן בכוכב (שיבוץ פוטנציאלי), 2 = אפס מסומן ב"פריים" (מועמד)
            var masks = new byte[h, w];
            var rowsCovered = new bool[h];
            var colsCovered = new bool[w];

            // שיבוץ כוכביות ראשוני: סימון אפסים בכוכביות ככל האפשר
            // אפשר לסמן אפס בכוכב רק אם השורה והעמודה שלו עדיין לא כוללות אפס מסומן
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (Math.Abs(costs[i, j]) < double.Epsilon && !rowsCovered[i] && !colsCovered[j])
                    {
                        masks[i, j] = 1;
                        rowsCovered[i] = true; // כיסוי השורה
                        colsCovered[j] = true; // כיסוי העמודה
                    }
                }
            }

            // ניקוי כל הכיסויים כהכנה לאלגוריתם המרכזי
            ClearAllCovers(rowsCovered, colsCovered, w, h);

            // שימוש במבנה זיכרון חסכוני יותר עבור נתיב ההרחבה
            // נעשה שימוש חוזר במערך הזה במהלך האלגוריתם במקום להקצות מחדש
            var path = new (int row, int column)[w * h];
            var pathStart = (-1, -1); // 

            // מכונת מצבים של האלגוריתם הראשי
            var step = 1;
            while (step != -1)
            {
                step = step switch
                {
                    1 => CheckForOptimalAssignment(masks, colsCovered, w, h),
                    2 => FindAndPrimeUncoveredZero(costs, masks, rowsCovered, colsCovered, w, h, ref pathStart),
                    3 => AugmentPathOfZeros(masks, rowsCovered, colsCovered, w, h, path, pathStart),
                    4 => CreateNewZerosByCostAdjustment(costs, rowsCovered, colsCovered, w, h),
                    _ => step
                };
            }

            // חילוץ ההקצאה הסופית ממטריצת המסכות
            var agentsTasks = new int[h];

            for (var i = 0; i < h; i++)
            {
                agentsTasks[i] = -1; // ברירת מחדל למצב ללא הקצאה
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1) // נמצא אפס מסומן בכוכבית (משימה)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                }
            }

            // אם ביצענו טרנספוזיציה של המטריצה ​​המקורית, עלינו טרנספוזיציה של התוצאה חזרה
            if (rowsGreaterThanCols)
            {
                var agentsTasksTranspose = new int[w];
                for (var i = 0; i < w; i++)
                {
                    agentsTasksTranspose[i] = -1;
                }

                for (var j = 0; j < h; j++)
                {
                    if (agentsTasks[j] != -1)
                    {
                        agentsTasksTranspose[agentsTasks[j]] = j;
                    }
                }
                agentsTasks = agentsTasksTranspose;
            }

            return agentsTasks;
        }

        // שלב 1: בדוק אם ההקצאה הנוכחית אופטימלית על ידי ספירת עמודות מכוסות.
        // אם כל העמודות מכוסות (כל אחת עם אפס מסומן בכוכבית), מצאנו פתרון אופטימלי.
        // מחזירה -1 אם ההקצאה אופטימלית, 2 אם עלינו להמשיך לשלב הבא
        private static int CheckForOptimalAssignment(byte[,] masks, bool[] colsCovered, int w, int h)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // כסו את כל העמודות המכילות אפס המסומן בכוכבית
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1) // אם התא מכיל אפס המסומן בכוכבית
                        colsCovered[j] = true; // כסה עמודה זו
                }
            }

            // ספירת העמודות המכוסות
            var colsCoveredCount = 0;
            for (var j = 0; j < w; j++)
            {
                if (colsCovered[j])
                    colsCoveredCount++;
            }

            // אם כל העמודות מכוסות, יש לנו הקצאה אופטימלית
            if (colsCoveredCount == Math.Min(w, h))
                return -1; // יציאה מהאלגוריתם - נמצא פתרון אופטימלי

            // אחרת, המשך לשלב 2
            return 2;
        }

        // שלב 2: מצא אפס לא מכוסה, קבע אותו כראשוני וקבע את השלב הבא.
        // אם יש אפס מסומן בכוכבית באותה שורה, כסה את השורה הזו וחשוף את העמודה של האפס המסומן בכוכבית.
        // אם אין אפס מסומן בכוכבית בשורה, המשך להרחבת הנתיב החל מאפס זה.
        // החזר 3 אם מצאנו אפס מסומן מראש ללא אפס מסומן בכוכבית בשורה שלו, 4 אם לא נותרו אפסים לא מכוסים.
        private static int FindAndPrimeUncoveredZero(double[,] costs, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, ref (int row, int column) pathStart)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            while (true)
            {
                // מצא אפס לא מכוסה
                var loc = FindUncoveredZero(costs, rowsCovered, colsCovered, w, h);
                if (loc.row == -1) // אם לא קיים 0 לא מכוסה
                    return 4; // המשך לשלב 4 - צריך ליצור אפסים חדשים

                // סמן כמועמד
                masks[loc.row, loc.column] = 2;

                //בדוק אם יש אפס המסומן בכוכבית באותה שורה
                var starCol = FindStarredZeroInRow(masks, w, loc.row);
                if (starCol != -1)
                {
                    // כסו את השורה של האפס הזה וחשפו את העמודה של האפס המסומן בכוכבית
                    // זה עוזר לנו למצוא עוד אפסים חשופים
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    // אין אפס מסומן בכוכבית בשורה זו פירושו שנוכל לבנות נתיב הרחבה
                    pathStart = loc;
                    return 3; // המשך לשלב 3 - הרחבת נתיב
                }
            }
        }

        // שלב 3: בנה והרחב נתיב של אפסים מסומנים בכוכבית ואפסים ראשוניים לסירוגין.
        // פעולה זו מגדילה את מספר האפסים המסומנים בכוכבית באחד, ומקרבת אותנו לפתרון האופטימלי.
        // החזר 1 כדי לחזור לשלב 1 לאחר הרחבת הנתיב
        private static int AugmentPathOfZeros(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, (int row, int column)[] path, (int row, int column) pathStart)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // בנה את נתיב ההרחבה
            var pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                // מצא אפס המסומן בכוכבית באותה עמודה כמו האפס האחרון בנתיב
                var row = FindStarredZeroInColumn(masks, h, path[pathIndex].column);
                if (row == -1) // אם אין אפס מסומן בכוכבית בעמודה זו
                    break; // בניית הנתיב הסתיימה

                // הוסף את האפס המסומן בכוכבית לנתיב
                pathIndex++;
                path[pathIndex] = (row, path[pathIndex - 1].column);

                // מצא אפס מסומן בכוכבית באותה שורה כמו האפס המסומן בכוכבית שהוספנו עכשיו
                var col = FindPrimedZeroInRow(masks, w, path[pathIndex].row);

                // הוסף את האפס הראשוני לנתיב
                pathIndex++;
                path[pathIndex] = (path[pathIndex - 1].row, col);
            }

            // הרחבת הנתיב: הפוך את האפסים המסומנים בכוכבית לאפסים לא מסומנים, ואת האפסים הראשוניים לאפסים מסומנים בכוכבית
            AugmentPath(masks, path, pathIndex + 1);

            // נקו את כל הכיסויים והפריימים כדי להתכונן לאיטרציה הבאה
            ClearAllCovers(rowsCovered, colsCovered, w, h);
            ClearAllPrimes(masks, w, h);

            // חזרה לשלב 1
            return 1;
        }

        // שלב 4: צור אפסים חדשים על ידי התאמת מטריצת העלות.
        // אנו מוסיפים את הערך המינימלי הלא מכוסה לכל השורות המכוסות ומחסירים אותו מכל העמודות הלא מכוסות.
        // פעולה זו יוצרת לפחות אפס חדש אחד ללא הפרעה לאפסים המסומנים בכוכבית.
        // החזר 2 כדי לחזור לשלב 2 לאחר יצירת אפסים חדשים
        private static int CreateNewZerosByCostAdjustment(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // מצא את הערך המינימלי הלא מכוסה במטריצת העלות
            var minValue = FindMinimumUncoveredValue(costs, rowsCovered, colsCovered, w, h);

            // עדכון מטריצת העלויות:
            // - הוסף את minValue לכל שורה מכוסה
            // - חיסור minValue מכל עמודה לא מכוסה
            // פעולה זו יוצרת לפחות אפס חדש אחד לא מכוסה
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (rowsCovered[i])
                        costs[i, j] += minValue;
                    if (!colsCovered[j])
                        costs[i, j] -= minValue;
                }
            }

            // חזור לשלב 2 כדי למצוא את האפס החדש שלא נחשף
            return 2;
        }

        // מוצא את הערך המינימלי בתאים הלא מכוסים של מטריצת העלות.
        private static double FindMinimumUncoveredValue(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            var minValue = double.MaxValue;

            // Search through all uncovered cells to find the minimum value
            for (var i = 0; i < h; i++)
            {
                if (rowsCovered[i]) continue; // Skip covered rows for performance

                for (var j = 0; j < w; j++)
                {
                    if (!colsCovered[j] && costs[i, j] < minValue)
                        minValue = costs[i, j];
                }
            }

            return minValue;
        }

        /// <summary>
        /// Finds a starred zero in the specified row.
        /// </summary>
        /// <returns>The column index of the starred zero, or -1 if none exists.</returns>
        private static int FindStarredZeroInRow(byte[,] masks, int w, int row)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var j = 0; j < w; j++)
            {
                if (masks[row, j] == 1)
                    return j;
            }

            return -1;
        }

        /// <summary>
        /// Finds a starred zero in the specified column.
        /// </summary>
        /// <returns>The row index of the starred zero, or -1 if none exists.</returns>
        private static int FindStarredZeroInColumn(byte[,] masks, int h, int col)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var i = 0; i < h; i++)
            {
                if (masks[i, col] == 1)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds a primed zero in the specified row.
        /// </summary>
        /// <returns>The column index of the primed zero, or -1 if none exists.</returns>
        private static int FindPrimedZeroInRow(byte[,] masks, int w, int row)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var j = 0; j < w; j++)
            {
                if (masks[row, j] == 2)
                    return j;
            }

            return -1;
        }

        /// <summary>
        /// Finds an uncovered zero in the cost matrix.
        /// </summary>
        /// <returns>The row and column of the uncovered zero, or (-1, -1) if none exists.</returns>
        private static (int row, int column) FindUncoveredZero(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            for (var i = 0; i < h; i++)
            {
                if (rowsCovered[i]) continue; // Skip covered rows for performance

                for (var j = 0; j < w; j++)
                {
                    if (Math.Abs(costs[i, j]) < double.Epsilon && !colsCovered[j])
                        return (i, j);
                }
            }

            return (-1, -1);
        }

        /// <summary>
        /// Converts stars and primes along the augmenting path:
        /// - Starred zeros become unstarred
        /// - Primed zeros become starred
        /// This effectively increases the number of starred zeros by one.
        /// </summary>
        private static void AugmentPath(byte[,] masks, (int row, int column)[] path, int pathLength)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            for (var i = 0; i < pathLength; i++)
            {
                var (row, column) = path[i];

                // Convert starred zeros to unstarred, and primed zeros to starred
                masks[row, column] = masks[row, column] switch
                {
                    1 => 0, // Unstar a starred zero
                    2 => 1, // Star a primed zero
                    _ => masks[row, column] // Leave other values unchanged
                };
            }
        }

        /// <summary>
        /// Clears all primed zeros in the mask matrix.
        /// </summary>
        private static void ClearAllPrimes(byte[,] masks, int w, int h)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 2)
                        masks[i, j] = 0;
                }
            }
        }


        // Uncovers all rows and columns.
        private static void ClearAllCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // Clear row covers
            for (var i = 0; i < h; i++)
            {
                rowsCovered[i] = false;
            }

            // Clear column covers
            for (var j = 0; j < w; j++)
            {
                colsCovered[j] = false;
            }
        }
    }
}