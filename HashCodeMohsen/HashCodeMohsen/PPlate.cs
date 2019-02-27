﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCodeMohsen
{
    class PPlate
    {

        private const int CHECK_SLICE_VALID = 0;
        private const int CHECK_SLICE_TOO_LOW = 1;
        private const int CHECK_SLICE_INVALID_SLICE = 2;
        private const int CHECK_SLICE_TOO_BIG = 3;

        private int mColumns;
        private int mRows;

        private int mMinIngPerSlice;
        private int mMaxSliceSize;

        private int[,] mPlate;

        public PPlate(int rows, int columns, int[,] plate, int minIng, int maxSliceSize)
        {
            mRows = rows;
            mColumns = columns;
            mPlate = plate;
            mMinIngPerSlice = minIng;
            mMaxSliceSize = maxSliceSize;
        }

        public int GetSize() { return mColumns * mRows; }

        public List<PSlice> PerformSlice()
        {
            int[,] plate = (int[,])mPlate.Clone();

            // Create greedy slicing. Iterating this phase did not yield better results
            List<PSlice> slices = PerformSlice_PhaseTwo(plate);

            return slices;
        }

        private List<PSlice> PerformSlice_PhaseTwo(int[,] plate)
        {
            int nextSliceId = -1;
            Dictionary<int, PSlice> sliceHash = new Dictionary<int, PSlice>();

            // Slice Pizza
            for (int r = 0; r < mRows; r++)
            {
                for (int c = 0; c < mColumns; c++)
                {
                    if (SlicePizzaAtPosition(plate, r, c, sliceHash, nextSliceId) == true)
                        nextSliceId--;
                }
            }

            // Try re-slicing
            List<PSlice> slices = new List<PSlice>(sliceHash.Values);
            foreach (PSlice slice in slices)
            {
                PSlice currentSlice = sliceHash[slice.ID];

                sliceHash.Remove(currentSlice.ID);
                currentSlice.RestoreSliceToPlate(plate, mPlate);

                SlicePizzaAtPosition(plate, currentSlice.RowMin, currentSlice.ColumnMin, sliceHash, currentSlice.ID);
            }


            return new List<PSlice>(sliceHash.Values);
        }

        private bool SlicePizzaAtPosition(int[,] plate, int r, int c, Dictionary<int, PSlice> sliceHash, int nextSliceId)
        {
            if (plate[r, c] < 0)
                return false;

            PSlice maxSlice = GetMaxSliceExtentionAt(plate, sliceHash, r, c, nextSliceId);
            if (maxSlice != null)
            {
                // Shrink existing slices
                Dictionary<int, int> sliceContent = maxSlice.GetSliceContent(plate);
                foreach (int overlapSliceId in sliceContent.Keys)
                {
                    if (overlapSliceId > 0)
                        continue;

                    PSlice existingSlice = sliceHash[overlapSliceId];
                    PSlice existingAfterOverlap = existingSlice.BuildShirnkedSliceWithOverlapping(maxSlice);
                    sliceHash[existingSlice.ID] = existingAfterOverlap;
                }

                maxSlice.RemoveSliceFromPlate(plate);
                sliceHash.Add(maxSlice.ID, maxSlice);

                return true;
            }

            return false;
        }

        private PSlice GetMaxSliceExtentionAt(int[,] plate, Dictionary<int, PSlice> sliceHash, int row, int column, int nextSliceId)
        {
            PSlice maxSlice = null;
            int maxSliceIngredients = 0;

            for (int minRow = row; minRow >= Math.Max(0, row - this.mMaxSliceSize); minRow--)
                for (int maxRow = row; maxRow < Math.Min(row + this.mMaxSliceSize + 1, mRows); maxRow++)
                {
                    for (int minCol = column; minCol >= Math.Max(0, column - this.mMaxSliceSize); minCol--)
                        for (int maxCol = column; maxCol < Math.Min(column + this.mMaxSliceSize + 1, mColumns); maxCol++)
                        {
                            int isValidSlice = IsValidSlice(this.mPlate, minRow, maxRow, minCol, maxCol);
                            if ((isValidSlice == CHECK_SLICE_TOO_BIG) || (isValidSlice == CHECK_SLICE_INVALID_SLICE))
                                break;

                            if (isValidSlice != CHECK_SLICE_VALID)
                                continue;

                            PSlice newSlice = new PSlice(nextSliceId, minRow, maxRow, minCol, maxCol);

                            // The new slice contains positions previously not in any slice
                            int newSliceIngredients = newSlice.CountIngredients(plate);
                            if (newSliceIngredients == 0)
                                continue;

                            // Check overlapping slices are still valid slices
                            Dictionary<int, int> sliceContent = newSlice.GetSliceContent(plate);
                            bool isValidOverlap = true;
                            foreach (int overlapSliceId in sliceContent.Keys)
                            {
                                if (overlapSliceId > 0)
                                    continue;

                                PSlice existingSlice = sliceHash[overlapSliceId];
                                PSlice existingAfterOverlap = existingSlice.BuildShirnkedSliceWithOverlapping(newSlice);
                                if (existingAfterOverlap == null)
                                {
                                    isValidOverlap = false;
                                    break;
                                }

                                if (this.IsValidSlice(this.mPlate,
                                    existingAfterOverlap.RowMin, existingAfterOverlap.RowMax,
                                    existingAfterOverlap.ColumnMin, existingAfterOverlap.ColumnMax) != CHECK_SLICE_VALID)
                                {
                                    isValidOverlap = false;
                                    break;
                                }
                            }
                            if (isValidOverlap == false)
                                continue;

                            // Check if the new slice is bettter than existing max
                            if (maxSlice == null)
                            {
                                maxSlice = newSlice;
                                maxSliceIngredients = newSliceIngredients;
                            }
                            else if (maxSliceIngredients < newSliceIngredients)
                            {
                                maxSlice = newSlice;
                                maxSliceIngredients = newSliceIngredients;
                            }
                        }
                }

            return maxSlice;
        }

        public bool IsValidSlicing(List<PSlice> slices)
        {
            int[,] plate = (int[,])mPlate.Clone();
            foreach (PSlice slice in slices)
            {
                if (IsValidSlice(mPlate, slice.RowMin, slice.RowMax, slice.ColumnMin, slice.ColumnMax) != CHECK_SLICE_VALID)
                    return false;

                for (int r = slice.RowMin; r <= slice.RowMax; r++)
                    for (int c = slice.ColumnMin; c <= slice.ColumnMax; c++)
                    {
                        if (plate[r, c] < 0)
                            return false;
                        plate[r, c] = slice.ID;
                    }
            }

            return true;
        }

        private int IsValidSlice(int[,] plate, int minRow, int maxRow, int minCol, int maxCol)
        {
            int count1 = 0;
            int count2 = 0;

            if ((maxRow - minRow + 1) * (maxCol - minCol + 1) > mMaxSliceSize)
                return CHECK_SLICE_TOO_BIG;

            for (int r = minRow; r <= maxRow; r++)
            {
                for (int c = minCol; c <= maxCol; c++)
                {
                    int plateVal = plate[r, c];
                    if (plateVal <= 0)
                        return CHECK_SLICE_INVALID_SLICE;
                    else if (plateVal == 1)
                        count1++;
                    else if (plateVal == 2)
                        count2++;
                    else
                        throw new Exception("Valid plate value: " + plateVal);
                }
            }

            if ((count1 < this.mMinIngPerSlice) || (count2 < this.mMinIngPerSlice))
                return CHECK_SLICE_TOO_LOW;

            return CHECK_SLICE_VALID;
        }
    }
}
