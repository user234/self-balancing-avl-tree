/////////////////////////////////////////////////////////////////////
// File Name               : AvlTree.cs
//      Created            : 12 7 2012   22:40
//      Author             : Costin S
//
/////////////////////////////////////////////////////////////////////

//---------------------------------------
// TREE_WITH_PARENT_POINTERS:
// Defines whether or not each node in the tree maintains a reference to its parent node
// To enable uncomment the following line


//#define TREE_WITH_PARENT_POINTERS

//---------------------------------------
// TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS: 
// Defines whether the tree exposes and implements concatenate and split operations
// In order to reduce space, you can do one of two things:
//      1. The simplest is to store both Balance and Height in one integer. Balance field needs only 2 bits which lefts 30 bits for the Height field. A tree with a HEIGHT > 2^30 (2 to the power of 30) is very unlikely you will ever build.
//      2. There is no need to maintain both Balance and Height for each node. Simple enough to modify and remove Balance field. Concat and Split were added as an afterthought after the implementation was already done using a Balance field.
//

#define TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SelfBalancedTrees
{
    /// <summary>
    /// Dictionary class
    /// </summary>
    /// <typeparam name="T">The type of the data stored in the nodes</typeparam>
    public class AVLTree<T> where T : IComparable<T>
    {
        internal delegate void VisitNodeHandler<TNode>(TNode node, int level);

        public enum SplitOperationMode
        {
            IncludeSplitValueToLeftSubtree,
            IncludeSplitValueToRightSubtree,
            DontIncludeSplitValue
        };

        /// <summary>
        /// Node class
        /// </summary>
        /// <typeparam name="TElem">The type of the elem.</typeparam>
        internal class Node<TElem> where TElem : IComparable<TElem>
        {
            #region Properties

            public Node<TElem> Left { get; set; }
            public Node<TElem> Right { get; set; }
            public TElem Data { get; set; }
            public int Balance { get; set; }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
            public int Height { get; set; }
#endif            

#if TREE_WITH_PARENT_POINTERS
            public Node<TElem> Parent { get; set; }
#endif

            #endregion

            #region Methods            

            /// <summary>
            /// Gets the height of the tree in log(n) time.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>The height of the tree. Runs in O(log(n)) where n is the number of nodes in the tree </returns>
            public static int getHeightLogN(Node<TElem> node)
            {
                if(node == null) 
                    return 0;
                else
                {
                    int leftHeight = getHeightLogN(node.Left);
                    if (node.Balance == 1)
                        leftHeight++;

                    return 1 + leftHeight;
                }
            }

            /// <summary>
            /// Adds the specified elem.
            /// </summary>
            /// <param name="elem">The elem.</param>
            /// <param name="data">The data.</param>
            /// <returns></returns>
            public static Node<TElem> add(Node<TElem> elem, TElem data, ref bool wasAdded)
            {
                if (elem == null)
                {
                    elem = new Node<TElem> { Data = data, Left = null, Right = null, Balance = 0 };

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                    elem.Height = 1;
#endif

                    wasAdded = true;
                }
                else
                {
                    if (data.CompareTo(elem.Data) < 0)
                    {                       
                        elem.Left = add(elem.Left, data, ref wasAdded);
                        if (wasAdded)
                        {
                            elem.Balance--;

                            if(elem.Balance == 0)
                                wasAdded = false;                                                           
                        }
                        
#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
#endif

                        if (elem.Balance == -2)
                        {
                            if (elem.Left.Balance == 1)
                            {
                                int elemLeftRightBalance = elem.Left.Right.Balance;

                                elem.Left = elem.Left.rotate_left();
                                elem = elem.rotate_right();

                                elem.Balance = 0;
                                elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;                                
                            }

                            else if (elem.Left.Balance == -1)
                            {
                                elem = elem.rotate_right();
                                elem.Balance = 0;
                                elem.Right.Balance = 0;
                            }
                            wasAdded = false;
                        }
                    }
                    else if (data.CompareTo(elem.Data) > 0)
                    {
                        elem.Right = add(elem.Right, data, ref wasAdded);
                        if (wasAdded)
                        {
                            elem.Balance++;
                            if (elem.Balance == 0)
                                wasAdded = false;
                        }

#if TREE_WITH_PARENT_POINTERS
                        elem.Right.Parent = elem;
#endif
                        if (elem.Balance == 2)
                        {
                            if (elem.Right.Balance == -1)
                            {
                                int elemRightLeftBalance = elem.Right.Left.Balance;

                                elem.Right = elem.Right.rotate_right();
                                elem = elem.rotate_left();

                                elem.Balance = 0;
                                elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;
                            }

                            else if (elem.Right.Balance == 1)
                            {
                                elem = elem.rotate_left();
                                
                                elem.Balance = 0;
                                elem.Left.Balance = 0;
                            }
                            wasAdded = false;
                        }
                    }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                    elem.Height = 1 + Math.Max(
                                            elem.Left != null ? elem.Left.Height : 0,
                                            elem.Right != null ? elem.Right.Height : 0
                                      );
#endif
                }
                return elem;
            }
          
            /// <summary>
            /// Searches the specified subtree.
            /// </summary>
            /// <param name="subtree">The subtree.</param>
            /// <param name="data">The data.</param>
            /// <returns></returns>
            public static Node<TElem> search(Node<TElem> subtree, TElem data)
            {
                if (subtree != null)
                {
                    if (data.CompareTo(subtree.Data) < 0)
                    {
                        return search(subtree.Left, data);
                    }
                    else if (data.CompareTo(subtree.Data) > 0)
                    {
                        return search(subtree.Right, data);
                    }
                    else
                    {
                        return subtree;
                    }
                }
                else return null;
            }            

            /// <summary>
            /// Left rotation. Precondition: (this.Right != null)
            /// </summary>
            /// <returns></returns>
            public Node<TElem> rotate_left()
            {
                var right = this.Right;
                Debug.Assert(this.Right != null);

                this.Right = right.Left;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                this.Height = 1 + Math.Max(
                                        this.Left != null ? this.Left.Height : 0,
                                        this.Right != null ? this.Right.Height : 0
                                    );
#endif

#if TREE_WITH_PARENT_POINTERS
                var parent = this.Parent;
                if (right.Left != null)
                {
                    right.Left.Parent = this;
                }
#endif
                right.Left = this;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                right.Height = 1 + Math.Max(
                                        right.Left != null ? right.Left.Height : 0,
                                        right.Right != null ? right.Right.Height : 0
                                );
#endif

#if TREE_WITH_PARENT_POINTERS
                this.Parent = right;
                if (parent != null)
                {
                    if (parent.Left == this)
                        parent.Left = right;
                    else
                        parent.Right = right;

                }
                right.Parent = parent;
#endif
                return right;

            }

            /// <summary>
            /// Right rotation. Precondition: (this.Left != null)
            /// </summary>
            /// <returns></returns>
            public Node<TElem> rotate_right()
            {
                var left = this.Left;
                Debug.Assert(this.Left != null);

                this.Left = left.Right;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                this.Height = 1 + Math.Max(
                                        this.Left != null ? this.Left.Height : 0,
                                        this.Right != null ? this.Right.Height : 0
                                    );
#endif 

#if TREE_WITH_PARENT_POINTERS
                var parent = this.Parent;
                if (left.Right != null)
                {
                    left.Right.Parent = this;
                }
#endif

                left.Right = this;

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                left.Height = 1 + Math.Max(
                                        left.Left != null ? left.Left.Height : 0,
                                        left.Right != null ? left.Right.Height : 0
                                    );
#endif

#if TREE_WITH_PARENT_POINTERS
                this.Parent = left;
                if (parent != null)
                {
                    if (parent.Left == this)
                        parent.Left = left;
                    else
                        parent.Right = left;

                }
                left.Parent = parent;
#endif
                return left;
            }

            /// <summary>
            /// Gets the parent node or null if this is the root. 
            /// </summary>
            /// <returns></returns>
            public Node<TElem> parent()
            {
#if TREE_WITH_PARENT_POINTERS
                return this.Parent;
#endif
                throw new NotImplementedException("can easily be implemented ..");
            }

            /// <summary>
            /// Finds the min.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns></returns>
            public static Node<TElem> findMin(Node<TElem> node)
            {
                while (node.Left != null)
                {
                    node = node.Left;
                }
                return node;
            }

            /// <summary>
            /// Finds the max.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns></returns>
            public static Node<TElem> findMax(Node<TElem> node)
            {
                while (node.Right != null)
                {
                    node = node.Right;
                }
                return node;
            }

            /// <summary>
            /// Deletes the min.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns></returns>
            public static Node<TElem> deleteMin(Node<TElem> node, ref bool wasDeleted)
            {
                if (node.Left == null)
                {
                    wasDeleted = true;
                    return node.Right;
                }

                node.Left = deleteMin(node.Left, ref wasDeleted);
                if (wasDeleted)
                    node.Balance++;

                #region Rebalancing
                
                if (wasDeleted)
                {
                    if (node.Balance == 1 || node.Balance == -1)
                    {
                        wasDeleted = false;
                    }
                    else if (node.Balance == -2)
                    {
                        if (node.Left.Balance == 1)
                        {
                            int leftRightBalance = node.Left.Right.Balance;

                            node.Left = node.Left.rotate_left();
                            node = node.rotate_right();

                            node.Balance = 0;
                            node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                            node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                        }
                        else if (node.Left.Balance == -1)
                        {
                            node = node.rotate_right();
                            node.Balance = 0;
                            node.Right.Balance = 0;
                        }
                        else if (node.Left.Balance == 0)
                        {
                            node = node.rotate_right();
                            node.Balance = 1;
                            node.Right.Balance = -1;

                            wasDeleted = false;
                        }
                    }
                    else if (node.Balance == 2)
                    {
                        if (node.Right.Balance == -1)
                        {
                            int rightLeftBalance = node.Right.Left.Balance;

                            node.Right = node.Right.rotate_right();
                            node = node.rotate_left();

                            node.Balance = 0;
                            node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                            node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                        }
                        else if (node.Right.Balance == 1)
                        {
                            node = node.rotate_left();
                            node.Balance = 0;
                            node.Left.Balance = 0;
                        }
                        else if (node.Right.Balance == 0)
                        {
                            node = node.rotate_left();
                            node.Balance = -1;
                            node.Left.Balance = 1;

                            wasDeleted = false;
                        }
                    }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                    node.Height = 1 + Math.Max(
                                            node.Left != null ? node.Left.Height : 0,
                                            node.Right != null ? node.Right.Height : 0
                                      );
#endif
                }

                #endregion

                return node;
            }

            /// <summary>
            /// Deletes the max.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns></returns>
            public static Node<TElem> deleteMax(Node<TElem> node, ref bool wasDeleted)
            {
                if (node.Right == null)
                {
                    wasDeleted = true;
                    return node.Left;
                }

                node.Right = deleteMax(node.Right, ref wasDeleted);
                if (wasDeleted)
                    node.Balance--;

                #region Rebalancing
                
                if (wasDeleted)
                {
                    if (node.Balance == 1 || node.Balance == -1)
                    {
                        wasDeleted = false;                        
                    }
                    else if (node.Balance == -2)
                    {
                        if (node.Left.Balance == 1)
                        {
                            int leftRightBalance = node.Left.Right.Balance;

                            node.Left = node.Left.rotate_left();
                            node = node.rotate_right();

                            node.Balance = 0;
                            node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                            node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
                        }
                        else if (node.Left.Balance == -1)
                        {
                            node = node.rotate_right();
                            node.Balance = 0;
                            node.Right.Balance = 0;
                        }
                        else if (node.Left.Balance == 0)
                        {
                            node = node.rotate_right();
                            node.Balance = 1;
                            node.Right.Balance = -1;

                            wasDeleted = false;
                        }
                    }
                    else if (node.Balance == 2)
                    {
                        if (node.Right.Balance == -1)
                        {
                            int rightLeftBalance = node.Right.Left.Balance;

                            node.Right = node.Right.rotate_right();
                            node = node.rotate_left();

                            node.Balance = 0;
                            node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                            node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                        }
                        else if (node.Right.Balance == 1)
                        {
                            node = node.rotate_left();
                            node.Balance = 0;
                            node.Left.Balance = 0;
                        }
                        else if (node.Right.Balance == 0)
                        {
                            node = node.rotate_left();
                            node.Balance = -1;
                            node.Left.Balance = 1;

                            wasDeleted = false;
                        }
                    }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                    node.Height = 1 + Math.Max(
                                            node.Left != null ? node.Left.Height : 0,
                                            node.Right != null ? node.Right.Height : 0
                                      );
#endif
                }

                #endregion

                return node;
            }

            /// <summary>
            /// Deletes the specified node.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <param name="arg">The arg.</param>
            /// <returns></returns>
            public static Node<TElem> delete(Node<TElem> node, TElem arg, ref bool wasDeleted)
            {
                int cmp = arg.CompareTo(node.Data);
                if (cmp < 0)
                {
                    if (node.Left != null)
                    {
                        node.Left = delete(node.Left, arg, ref wasDeleted);
                                                
                        if(wasDeleted)
                            node.Balance++;
                    }
                }
                else if (cmp == 0)
                {
                    wasDeleted = true;
                    if (node.Left != null && node.Right != null)
                    {
                        var min = findMin(node.Right);
                        TElem data = node.Data;
                        node.Data = min.Data;
                        min.Data = data;
                        
                        wasDeleted = false;
                        node.Right = delete(node.Right, data, ref wasDeleted);
                       
                        if (wasDeleted) 
                            node.Balance--;
                    }
                    else if (node.Left == null)
                    {
                        return node.Right;
                    }
                    else
                    {
                        return node.Left;
                    }
                }
                else //cmp > 0
                {
                    if (node.Right != null)
                    {
                        node.Right = delete(node.Right, arg, ref wasDeleted);
                        if (wasDeleted)
                            node.Balance--;
                    }
                }

                if(wasDeleted)
                {
                    if (node.Balance == 1 || node.Balance == -1)
                    {
                        wasDeleted = false;
                    }
                    else if (node.Balance == -2)
                    {
                        if (node.Left.Balance == 1)
                        {
                            int leftRightBalance = node.Left.Right.Balance;

                            node.Left = node.Left.rotate_left();
                            node = node.rotate_right();

                            node.Balance = 0;
                            node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
                            node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;                            
                        }
                        else if (node.Left.Balance == -1)
                        {
                            node = node.rotate_right();
                            node.Balance = 0;
                            node.Right.Balance = 0;
                        }
                        else if (node.Left.Balance == 0)
                        {
                            node = node.rotate_right();
                            node.Balance = 1;
                            node.Right.Balance = -1;

                            wasDeleted = false;
                        }
                    }
                    else if (node.Balance == 2)
                    {
                        if (node.Right.Balance == -1)
                        {
                            int rightLeftBalance = node.Right.Left.Balance;

                            node.Right = node.Right.rotate_right();
                            node = node.rotate_left();

                            node.Balance = 0;
                            node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
                            node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
                        }
                        else if (node.Right.Balance == 1)
                        {
                            node = node.rotate_left();
                            node.Balance = 0;
                            node.Left.Balance = 0;
                        }
                        else if (node.Right.Balance == 0)
                        {
                            node = node.rotate_left();
                            node.Balance = -1;
                            node.Left.Balance = 1;

                            wasDeleted = false;
                        }
                    }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS
                    node.Height = 1 + Math.Max(
                                            node.Left != null ? node.Left.Height : 0,
                                            node.Right != null ? node.Right.Height : 0
                                      );
#endif
                }
                return node;
            }

            /// <summary>
            /// visitor helper for debugging purposes
            /// </summary>
            /// <param name="visitor">The visitor.</param>
            /// <param name="level">The level.</param>
            internal void visit_inorder(VisitNodeHandler<Node<TElem>> visitor, int level)
            {
                if (Left != null) Left.visit_inorder(visitor, level + 1);

                visitor(this, level);

                if (Right != null) Right.visit_inorder(visitor, level + 1);
            }                        


#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

            /// <summary>
            /// Concatenates the elements of the two trees. 
            /// Precondition: ALL elements of node2 must be LARGER than all elements of node1.
            /// </summary>
            /// <remarks>
            /// Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). Runs in O(height(node1)) if height(node1) == height(node2).
            /// Performing find/delete in one operation gives O(height(node1)) speed.
            /// </remarks>
            public static Node<TElem> concat(Node<TElem> node1, Node<TElem> node2)
            {
                if (node1 == null)
                {
                    return node2;
                }
                else if (node2 == null)
                {
                    return node1;
                }
                else
                {
                    bool wasAdded = false, wasDeleted = false;

                    int height1 = node1.Height;
                    int height2 = node2.Height;
                    
                    if (height1 == height2)
                    {
                        var result = new Node<TElem>() { Data = default(TElem), Left = node1, Right = node2, Balance = 0, Height = 1 + height1 };
                        result = Node<TElem>.delete(result, default(TElem), ref wasDeleted);
                        return result;
                    }
                    else if (height1 > height2)
                    {
                        var min = Node<TElem>.findMin(node2);
                        node2 = Node<TElem>.deleteMin(node2, ref wasDeleted);
                        height2 = node2.Height;

                        if (node2 != null)
                        {
                            node1 = Node<TElem>.concat_impl(node1, height1, node2, height2, min.Data, ref wasAdded);
                        }
                        else
                        {
                            node1 = Node<TElem>.add(node1, min.Data, ref wasAdded);
                        }
                        return node1;
                    }
                    else
                    {                        
                        var max = Node<TElem>.findMax(node1);
                        node1 = Node<TElem>.deleteMax(node1, ref wasDeleted);
                        height1 = node1.Height;

                        if (node1 != null)
                        {
                            node2 = Node<TElem>.concat_impl(node2, height2, node1, height1, max.Data, ref wasAdded);
                        }
                        else
                        {
                            node2 = Node<TElem>.add(node2, max.Data, ref wasAdded);
                        }
                        return node2;
                    }
                }
            }

            /// <summary>
            /// Concatenates the specified trees. 
            /// Precondition is that height(elem) > height(elem2add)
            /// </summary>
            /// <param name="elem">The elem</param>
            /// <param name="elemHeight">Height of the elem.</param>
            /// <param name="elem2add">The elem2add.</param>
            /// <param name="elem2AddHeight">Height of the elem2 add.</param>
            /// <param name="newData">The new data.</param>
            /// <param name="wasAdded">if set to <c>true</c> [was added].</param>
            /// <returns></returns>
            private static Node<TElem> concat_impl( Node<TElem> elem, int elemHeight, 
                                                    Node<TElem> elem2add, int elem2AddHeight,
                                                    TElem newData, ref bool wasAdded)
            {
                int heightDifference = elemHeight - elem2AddHeight;

                if (elem == null)
                {
                    if(heightDifference > 0)
                    {
                        throw new ArgumentException("invalid input");
                    }
                }
                else 
                {
                    if(elem.Data.CompareTo(newData) < 0)
                    {
                        if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == -1))
                        {
                            int balance = elem2AddHeight - elemHeight;
                            Debug.Assert(Math.Abs(balance) < 2);

                            elem = new Node<TElem>() { Data = newData, Left = elem, Right = elem2add, Balance = balance };
                            wasAdded = true;

#if TREE_WITH_PARENT_POINTERS
                            elem.Left.Parent = elem;
                            elem2add.Parent = elem;
#endif
                        }
                        else
                        {
                            int elemRightHeight = (elem.Balance == -1) ? elemHeight - 2 : elemHeight - 1;                            
                            heightDifference = elemRightHeight - elem2AddHeight;

                            elem.Right = concat_impl(elem.Right, elemRightHeight, elem2add, elem2AddHeight, newData, ref wasAdded);
                        
                            if (wasAdded)
                            {
                                elem.Balance++;
                                if (elem.Balance == 0)
                                    wasAdded = false;
                            }

#if TREE_WITH_PARENT_POINTERS
                            elem.Right.Parent = elem;
#endif
                            if (elem.Balance == 2)
                            {                                
                                if (elem.Right.Balance == -1)
                                {
                                    int elemRightLeftBalance = elem.Right.Left.Balance;

                                    elem.Right = elem.Right.rotate_right();
                                    elem = elem.rotate_left();

                                    elem.Balance = 0;
                                    elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
                                    elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;

                                    wasAdded = false;
                                }
                                else if (elem.Right.Balance == 1)
                                {
                                    elem = elem.rotate_left();

                                    elem.Balance = 0;
                                    elem.Left.Balance = 0;

                                    wasAdded = false;
                                }
                                else if (elem.Right.Balance == 0)
                                {
                                    //special case for concat ..
                                    //balancing of the newly added subtree is usually done as part of the adding subroutine,.., this situation is therefore not present in the adding procedure .. cater for it here..

                                    elem = elem.rotate_left();

                                    elem.Balance = -1;
                                    elem.Left.Balance = 1;
                                    
                                    wasAdded = true;
                                }
                            }                            
                        }                                                
                    }
                    else if (elem.Data.CompareTo(newData) > 0)
                    {
                        if (heightDifference == 0 || (heightDifference == 1 && elem.Balance == 1))
                        {
                            int balance = elemHeight - elem2AddHeight;
                            Debug.Assert(Math.Abs(balance) < 2);

                            elem = new Node<TElem>() { Data = newData, Left = elem2add, Right = elem, Balance = balance  };
                            wasAdded = true;

#if TREE_WITH_PARENT_POINTERS                            
                            elem.Right.Parent = elem;
                            elem2add.Parent = elem;
#endif
                        }
                        else
                        {
                            int elemLeftHeight = (elem.Balance == 1) ? elemHeight - 2 : elemHeight - 1;
                            heightDifference = elemLeftHeight - elem2AddHeight;

                            elem.Left = concat_impl(elem.Left, elemLeftHeight, elem2add, elem2AddHeight, newData, ref wasAdded);

                            if (wasAdded)
                            {
                                elem.Balance--;
                                if (elem.Balance == 0)
                                    wasAdded = false;
                            }

#if TREE_WITH_PARENT_POINTERS
                        elem.Left.Parent = elem;
#endif
                            if (elem.Balance == -2)
                            {
                                if (elem.Left.Balance == 1)
                                {
                                    int elemLeftRightBalance = elem.Left.Right.Balance;

                                    elem.Left = elem.Left.rotate_left();
                                    elem = elem.rotate_right();

                                    elem.Balance = 0;
                                    elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
                                    elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;

                                    wasAdded = false;
                                }
                                else if (elem.Left.Balance == -1)
                                {
                                    elem = elem.rotate_right();
                                    elem.Balance = 0;
                                    elem.Right.Balance = 0;

                                    wasAdded = false;
                                }
                                else if (elem.Left.Balance == 0)
                                {
                                    //special case for concat ..
                                    //balancing of the newly added subtree is usually done as part of the adding subroutine,.., this situation is therefore not present in the adding procedure .. cater for it here..

                                    elem = elem.rotate_right();

                                    elem.Balance = 1;
                                    elem.Right.Balance = -1;

                                    wasAdded = true;
                                }
                            }
                        }                        
                    }

                    elem.Height = 1 + Math.Max(
                                            elem.Left != null ? elem.Left.Height : 0,
                                            elem.Right != null ? elem.Right.Height : 0
                                      );
                }
                return elem;
            }

            /// <summary>
            /// This routine is used by the split procedure. Similar to concat except that the junction point is specified (i.e. the 'value' argument).
            /// ALL nodes in node1 tree have values less than the 'value' argument and ALL nodes in node2 tree have values greater than 'value'.
            /// O(log N). Consider merging concat and concat_at_point 
            /// </summary>
            private static Node<TElem> concat_at_point(Node<TElem> node1, Node<TElem> node2, TElem value)
            {
                bool wasAdded = false;

                if (node1 == null)
                {
                    if (node2 != null)
                    {
                        node2 = Node<TElem>.add(node2, value, ref wasAdded);
                    }
                    else
                    {
                        node2 = new Node<TElem> { Data = value, Balance = 0, Left = null, Right = null, Height = 1 };
                    }
                    return node2;
                }
                else if (node2 == null)
                {
                    if (node1 != null)
                    {
                        node1 = Node<TElem>.add(node1, value, ref wasAdded);
                    }
                    else
                    {
                        node1 = new Node<TElem> { Data = value, Balance = 0, Left = null, Right = null , Height = 1 };
                    }
                    return node1;
                }
                else
                {
                    int height1 = node1.Height;
                    int height2 = node2.Height;
                    
                    if (height1 == height2)
                    {
                        // construct a new tree with its left and right subtrees pointing to the trees to be concatenated
                        return new Node<TElem>() { Data = value, Left = node1, Right = node2, Balance = 0 , Height = 1 + height1 };                        
                    }
                    else if (height1 > height2)
                    {
                        // walk on node1's rightmost edge until you find the right place to insert the subtree with the smaller height (i.e. node2)
                        return Node<TElem>.concat_impl(node1, height1, node2, height2, value, ref wasAdded);                        
                    }
                    else
                    {
                        // walk on node2's leftmost edge until you find the right place to insert the subtree with the smaller height (i.e. node1)
                        return Node<TElem>.concat_impl(node2, height2, node1, height1, value, ref wasAdded);
                    }
                }
            }
             
            /// <summary>
            /// Splits this avl tree instance into two avl subtrees by the specified value.
            /// </summary>
            /// <param name="value">The value to use when splitting this instance.</param>
            /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, include it in the left or include it in the right tree respectively</param>
            /// <param name="splitLeftTree">The split left avl tree. All values of this subtree are less than the value argument.</param>
            /// <param name="splitRightTree">The split right avl tree. All values of this subtree are greater than the value argument.</param>
            /// <returns></returns>
            public static Node<TElem> split(Node<TElem> elem, TElem data,
                                            ref Node<TElem> splitLeftTree,
                                            ref Node<TElem> splitRightTree,
                                            SplitOperationMode mode,
                                            ref bool wasFound)
            {
                bool wasAdded = false;
                if (data.CompareTo(elem.Data) < 0)
                {
                    elem.Left = split(elem.Left, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                    if (wasFound)
                    {
                        splitRightTree = Node<TElem>.concat_at_point(splitRightTree, elem.Right, elem.Data);
                    }
                }
                else if (data.CompareTo(elem.Data) > 0)
                {
                    elem.Right = split(elem.Right, data, ref splitLeftTree, ref splitRightTree, mode, ref wasFound);
                    if (wasFound)
                    {
                        splitLeftTree = Node<TElem>.concat_at_point(elem.Left, splitLeftTree, elem.Data);
                    }
                }
                else
                {
                    wasFound = true;
                    splitLeftTree = elem.Left;
                    splitRightTree = elem.Right;

                    switch (mode)
                    {
                        case SplitOperationMode.IncludeSplitValueToLeftSubtree:
                            splitLeftTree = Node<TElem>.add(splitLeftTree, elem.Data, ref wasAdded);
                            break;
                        case SplitOperationMode.IncludeSplitValueToRightSubtree:
                            splitRightTree = Node<TElem>.add(splitRightTree, elem.Data, ref wasAdded);
                            break;
                    }
                }
                return elem;
            }

#endif

            #endregion
        }

        #region Members

        private Node<T> Root { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AVLTree&lt;T&gt;"/> class.
        /// </summary>
        public AVLTree()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AVLTree&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="elems">The elems.</param>
        public AVLTree(IEnumerable<T> elems)
        {
            if (elems != null)
            {
                foreach (var elem in elems)
                {
                    this.add(elem);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified arg.
        /// </summary>
        /// <param name="arg">The arg.</param>
        public void add(T arg)
        {
            bool wasAdded = false;
            this.Root = Node<T>.add(this.Root, arg, ref wasAdded);
        }

        /// <summary>
        /// Deletes the specified arg.
        /// </summary>
        /// <param name="arg">The arg.</param>
        public void delete(T arg)
        {
            if (this.Root != null)
            {
                bool wasDeleted = false;
                this.Root = Node<T>.delete(this.Root, arg, ref wasDeleted);
            }
        }

        /// <summary>
        /// Deletes the min.
        /// </summary>
        public void deleteMin()
        {
            if (this.Root != null)
            {
                bool wasDeleted = false;
                this.Root = Node<T>.deleteMin(this.Root, ref wasDeleted);
            }
        }

        /// <summary>
        /// Gets the min.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool getMin(out T value)
        {
            if (this.Root != null)
            {
                var min = Node<T>.findMin(this.Root);
                if (min != null)
                {
                    value = min.Data;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Gets the max.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>        
        public bool getMax(out T value)
        {
            if (this.Root != null)
            {
                var max = Node<T>.findMax(this.Root);
                if (max != null)
                {
                    value = max.Data;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        /// Determines whether tree contains the specified argument.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <returns>
        ///   <c>true</c> if tree contains the specified arg; otherwise, <c>false</c>.
        /// </returns>
        public bool contains(T arg)
        {
            return Node<T>.search(this.Root, arg) != null;
        }        

        /// <summary>
        /// Deletes the max.
        /// </summary>
        public void deleteMax()
        {
            if (this.Root != null)
            {
                bool wasDeleted = false;
                this.Root = Node<T>.deleteMax(this.Root, ref wasDeleted);
            }
        }

#if TREE_WITH_CONCAT_AND_SPLIT_OPERATIONS

        /// <summary>
        /// Concatenates the elements of the two trees. Precondition: ALL values in 'other' must be LARGER than this tree's values
        /// Operation is destructive.
        /// </summary>
        /// <remarks>
        /// Assuming height(node1) > height(node2), our procedure runs in height(node1) + height(node2) due to the two calls to findMin/deleteMin (or findMax, deleteMax respectively). Runs in O(height(node1)) if height(node1) == height(node2).
        /// Can be sped up.
        /// </remarks>
        public AVLTree<T> concat(AVLTree<T> other)
        {
            var root = Node<T>.concat(this.Root, other.Root);            
            if(root != null)
            {
                return new AVLTree<T>() { Root = root };
            }

            return null;
        }

        /// <summary>
        /// Splits this avl tree instance into two avl subtrees by the specified value. Operation is destructive.
        /// </summary>
        /// <param name="value">The value to use when splitting this instance.</param>
        /// <param name="mode">The mode specifying what to do with the value used for splitting. Options are not to include this value in either of the two resulting trees, include it in the left or include it in the right tree respectively</param>
        /// <param name="splitLeftTree">The split left avl tree. All values of this subtree are less than the value argument.</param>
        /// <param name="splitRightTree">The split right avl tree. All values of this subtree are greater than the value argument.</param>
        /// <returns></returns>
        public bool split(T value, SplitOperationMode mode, out AVLTree<T> splitLeftTree, out AVLTree<T> splitRightTree)
        {
            splitLeftTree = null;
            splitRightTree = null;
                        
            Node<T> splitLeftRoot = null, splitRightRoot = null;
            bool wasFound = false;

            Node<T>.split(this.Root, value, ref splitLeftRoot, ref splitRightRoot, mode, ref wasFound);
            if (wasFound)
            {
                splitLeftTree = new AVLTree<T>() { Root = splitLeftRoot };
                splitRightTree = new AVLTree<T>() { Root = splitRightRoot };
            }

            return wasFound;            
        }

#endif

        /// <summary>
        /// Returns the height of the tree in O(log N).
        /// </summary>
        /// <returns></returns>
        public int getHeightLogN()
        {
            return Node<T>.getHeightLogN(this.Root);
        }

        /// <summary>
        /// visitor helper for debugging only.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        internal void visit_inorder(VisitNodeHandler<Node<T>> visitor)
        {
            if (this.Root != null)
            {
                this.Root.visit_inorder(visitor, 0);
            }
        }

        #endregion
    }
}
