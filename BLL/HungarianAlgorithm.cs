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

            // Extract the final assignment from the masks matrix
            var agentsTasks = new int[h];

            for (var i = 0; i < h; i++)
            {
                agentsTasks[i] = -1; // Default to unassigned
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1) // Found a starred zero (assignment)
                    {
                        agentsTasks[i] = j;
                        break;
                    }
                }
            }

            // If we transposed the original matrix, we need to transpose the result back
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

        /// <summary>
        /// Step 1: Check if the current assignment is optimal by counting covered columns.
        /// If all columns are covered (each with a starred zero), we have found an optimal solution.
        /// </summary>
        /// <returns>-1 if assignment is optimal, 2 if we need to proceed to next step</returns>
        private static int CheckForOptimalAssignment(byte[,] masks, bool[] colsCovered, int w, int h)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // Cover all columns containing a starred zero
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    if (masks[i, j] == 1) // If cell contains a starred zero
                        colsCovered[j] = true; // Cover this column
                }
            }

            // Count covered columns
            var colsCoveredCount = 0;
            for (var j = 0; j < w; j++)
            {
                if (colsCovered[j])
                    colsCoveredCount++;
            }

            // If all columns are covered, we have an optimal assignment
            if (colsCoveredCount == Math.Min(w, h))
                return -1; // Exit the algorithm - optimal solution found

            // Otherwise, proceed to step 2
            return 2;
        }

        /// <summary>
        /// Step 2: Find an uncovered zero, prime it, and determine the next step.
        /// If there's a starred zero in the same row, cover that row and uncover the column of the starred zero.
        /// If there's no starred zero in the row, proceed to augment the path starting from this zero.
        /// </summary>
        /// <returns>3 if we found a primed zero with no starred zero in its row, 4 if no uncovered zeros remain</returns>
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
                // Find an uncovered zero
                var loc = FindUncoveredZero(costs, rowsCovered, colsCovered, w, h);
                if (loc.row == -1) // If no uncovered zero exists
                    return 4; // Proceed to step 4 - need to create new zeros

                // Prime the found zero (mark as a candidate)
                masks[loc.row, loc.column] = 2;

                // Check if there's a starred zero in the same row
                var starCol = FindStarredZeroInRow(masks, w, loc.row);
                if (starCol != -1)
                {
                    // Cover the row of this zero and uncover the column of the starred zero
                    // This helps us find more uncovered zeros
                    rowsCovered[loc.row] = true;
                    colsCovered[starCol] = false;
                }
                else
                {
                    // No starred zero in this row means we can build an augmenting path
                    pathStart = loc;
                    return 3; // Proceed to step 3 - augment path
                }
            }
        }

        /// <summary>
        /// Step 3: Construct and augment a path of alternating starred and primed zeros.
        /// This increases the number of starred zeros by one, bringing us closer to the optimal solution.
        /// </summary>
        /// <returns>1 to return to step 1 after augmenting the path</returns>
        private static int AugmentPathOfZeros(byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h, (int row, int column)[] path, (int row, int column) pathStart)
        {
            if (masks == null)
                throw new ArgumentNullException(nameof(masks));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // Construct the augmenting path
            var pathIndex = 0;
            path[0] = pathStart;

            while (true)
            {
                // Find a starred zero in the same column as the last zero in the path
                var row = FindStarredZeroInColumn(masks, h, path[pathIndex].column);
                if (row == -1) // If no starred zero in this column
                    break; // Path construction is complete

                // Add the starred zero to the path
                pathIndex++;
                path[pathIndex] = (row, path[pathIndex - 1].column);

                // Find a primed zero in the same row as the starred zero we just added
                var col = FindPrimedZeroInRow(masks, w, path[pathIndex].row);

                // Add the primed zero to the path
                pathIndex++;
                path[pathIndex] = (path[pathIndex - 1].row, col);
            }

            // Augment the path: convert stars to non-stars and primes to stars
            AugmentPath(masks, path, pathIndex + 1);

            // Clear all covers and primes to prepare for the next iteration
            ClearAllCovers(rowsCovered, colsCovered, w, h);
            ClearAllPrimes(masks, w, h);

            // Return to step 1
            return 1;
        }

        /// <summary>
        /// Step 4: Create new zeros by adjusting the cost matrix.
        /// We add the minimum uncovered value to all covered rows and subtract it from all uncovered columns.
        /// This creates at least one new uncovered zero without disturbing the existing starred zeros.
        /// </summary>
        /// <returns>2 to return to step 2 after creating new zeros</returns>
        private static int CreateNewZerosByCostAdjustment(double[,] costs, bool[] rowsCovered, bool[] colsCovered, int w, int h)
        {
            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            if (rowsCovered == null)
                throw new ArgumentNullException(nameof(rowsCovered));

            if (colsCovered == null)
                throw new ArgumentNullException(nameof(colsCovered));

            // Find the minimum uncovered value in the cost matrix
            var minValue = FindMinimumUncoveredValue(costs, rowsCovered, colsCovered, w, h);

            // Update the cost matrix:
            // - Add minValue to every covered row
            // - Subtract minValue from every uncovered column
            // This creates at least one new uncovered zero
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

            // Return to step 2 to find the new uncovered zero
            return 2;
        }

        /// <summary>
        /// Finds the minimum value in the uncovered cells of the cost matrix.
        /// </summary>
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

        /// <summary>
        /// Uncovers all rows and columns.
        /// </summary>
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