import argparse
import matplotlib.pyplot as plt
import networkx as nx
import os
import random
import re
import sys

#Regular expressions.
FILE_REGEX = 'mandelbrot-(?P<number>[0-9]*)-network.txt'
FLOAT_REGEX = '\-{0,1}[0-9]*\.{0,1}[0-9]*'
COMPLEX_REGEX = r'\((?P<real>{0}), (?P<imaginary>{0})\)'.format(FLOAT_REGEX)
PY_COMPLEX_REGEX = r'\((?P<complex>[^\(, ^\)]*)\)'
OUTPUT_NODE_REGEX = r'(H|O)\([0-9]*, (?P<function>.*)\)'

#Separator for elements in a link.
SEPARATOR = "( - | -> )" 

#The size of the nodes in the graph.
NODE_SIZE = 2800

#The size of the font for the node labels. 
NODE_FONT_SIZE = 14 

#The labels of the input nodes.
INPUT_LABELS = {'B(1)': '1', 'I(2)': 'z', 'I(3)': 'c'}

#The input nodes.
INPUT_NODES = INPUT_LABELS.keys()

#The base distance between each node.
NODE_DISTANCE = 1

#The colour of the node.
NODE_COLOUR = 'w'

#Tweaks to the starting coordinates of specific (graph, depth) pairs.
X_POS_TWEAKS =  {(1, 2): 1, (1, 3): 1, (3, 2): 0.33, (6, 2): -0.25, (12, 2): 0.33, 
        (12, 3): 0.25, (13, 2): 1, (13, 3): 0.67, (13, 4): 1, (16, 2): 1, 
        (27, 2): 0.5, (29, 2): 0.4, (29, 3): 0.25, (32, 2): 0.5, (32, 3): -0.25, 
        (33, 2): 0.5, (32, 3): 0.25, (33, 4): 0.25, (33, 5): 1, (35, 2): -0.5, 
        (35, 3): -0.75, (35, 4): -1.5, (37, 2): 0.4, (37, 3): 0.25, (42, 2): -0.5, 
        (42, 3): -1, (42, 4): -1.5, (47, 2): -0.25, (48, 2): -0.25, (49, 2): -0.75, 
        (49, 3): -0.25}

Y_POS_TWEAKS =  {(4, 2): -0.5, (4, 3): -0.25, (12, 2): -0.5, (12, 3): -0.25, 
        (13, 2): -0.5, (29, 3): 0.25, (29, 2): -0.5, (32, 2): -0.75, (32, 3): -0.5, 
        (33, 2): -1.25, (33, 3): -1.75, (33, 4): -1.25, (49, 2): -1.5, (49, 3): -0.32}

HORIZ_DIST_TWEAKS = {(4, 3): 0.5, (4, 2): 0.5, (6, 2): 0.5}

#Indexes of the link elements.
#The index containing the from node.
FROM_INDEX = 0

#The index containing the to node.
TO_INDEX = 4

#The index containing the link weight.
WEIGHT_INDEX = 2

#The dimensions of the pyplot graph.
IMAGE_WIDTH = 15
IMAGE_HEIGHT = 15


def main(path, to_screen = False, filter_number = None):
    """Outputs the graphs to the given file [path] or draws them on screen if
    [to_screen] is true.
    """

    #For each (file name, fractal number) pair.
    for filename, number in sorted(parse_filenames(path), 
                                    lambda x, y : cmp(x[1], y[1])):

        #Filter a specific number if one has been requested. 
        if filter_number is not None and number != filter_number:
            continue

        #Inform the user that the graph is being drawn.
        print("Drawing graph {0}".format(number))

        #Maps a depth index to the nodes at that depth.
        depth_map = {}

        with open(os.path.join(path, filename), 'r') as filebuf:
            graph = nx.DiGraph()
            
            edge_labels = {}
            node_labels = {}

            #The positions of the nodes on the graph image.
            positions = {}

            #For each line in the graph description.
            for line in filebuf:
                #Interpret the line.
                frm, to, weight = parse_edge(line)
                graph.add_edge(frm, to)

                #Format the complex weight.
                weight_string = str(weight)
                match = re.match(PY_COMPLEX_REGEX, weight_string)

                weight_string = (
                    match.group('complex') if match else weight_string 
                )

                edge_labels[(frm, to)] = weight_string.replace('j', 'i')

            #For each node.
            for node in graph.nodes():
                #Add it to the depth map and find its label.
                depth_index = 0
                
                if not INPUT_LABELS.has_key(node):
                    node_labels[node] = output_node_label(node)

                    depth_index = max_length_to_input_nodes(graph, node)

                else:
                    node_labels[node] = INPUT_LABELS[node]

                if not depth_map.has_key(depth_index):
                    depth_map[depth_index] = []

                depth_map[depth_index].append(node)

            #The y coordinate of the bottom-most node in this graph image.
            start_pos_y = -(len(depth_map.keys()) * NODE_DISTANCE) / 2.0 

            #Process each depth level.
            for depth_index, nodes in depth_map.items():
                num_of_nodes = len(nodes)
                tweak_id = (number, depth_index)

                #The distance between the nodes at this level.
                horiz_dist = HORIZ_DIST_TWEAKS.get(tweak_id) or NODE_DISTANCE

                #The y offset of the node according to the tweak.
                y_offset = (Y_POS_TWEAKS.get(tweak_id) or 0)

                #The x coordinate of the left-most node in this level, taking
                #into account the tweak.
                start_pos_x = (-(num_of_nodes - 1) * horiz_dist / 2.0 
                                + (X_POS_TWEAKS.get(tweak_id) or 0))

                #Position all of the nodes at this level.
                for i in xrange(num_of_nodes):
                    node = nodes[i]

                    positions[node] = (start_pos_x + i * horiz_dist,
                                        start_pos_y + depth_index * 
                                        NODE_DISTANCE + y_offset)
                


            #Draw the graph.
            plt.figure(figsize=(IMAGE_WIDTH, IMAGE_HEIGHT))


            nx.draw_networkx(graph, positions, node_size = NODE_SIZE, 
                            labels = node_labels, font_size = NODE_FONT_SIZE,
                            node_color = NODE_COLOUR)
            
            nx.draw_networkx_edge_labels(graph, positions, rotate=False, 
                                        edge_labels = edge_labels, font_size=12)

            if to_screen:
                plt.show()
            else:
                plt.savefig('{0}/mandelbrot-{1}-network-graph.png'
                                .format(path, number))

def max_length_to_input_nodes(graph, node):
    """Returns the maximum length from any node in the input nodes to [node] 
    within [graph]."""

    try:
        return max(max_length_to_input_node(graph, node, input_node) 
                    for input_node in INPUT_NODES if input_node in graph.nodes())
    except ValueError:
        return 0

def max_length_to_input_node(graph, node, input_node):
    """Returns the maximum length from [input_node] to [node] within [graph]."""

    try:
        return max(len(path) for path in 
                    nx.all_simple_paths(graph, input_node, node))
    except ValueError:
        return 0

def parse_complex(complex_string):
    """Parses a complex number string within a graph description."""

    m = re.match(COMPLEX_REGEX, complex_string)

    return complex(float(m.group('real')), float(m.group('imaginary')))


def parse_edge(edge_string):
    """Parses an edge string within a graph description."""

    elements = [el.strip() for el in re.split(SEPARATOR, edge_string)]

    return (elements[FROM_INDEX], elements[TO_INDEX],
                parse_complex(elements[WEIGHT_INDEX]))

def output_node_label(node_description):
    """Returns the label for the output node with the given description."""

    return re.match(OUTPUT_NODE_REGEX, node_description).group('function')

def parse_filenames(path):
    """Generates (filename, fractal number) for the graph descriptions in the 
    given path."""

    for filename in os.listdir(path):
        match = re.match(FILE_REGEX, filename)

        if match is not None:
            yield filename, int(match.group('number'))

if __name__ == '__main__':
    #Parse command line arguments.
    parser = argparse.ArgumentParser(
            description='Generates graphs from network description '
                'in mandelbrot-[number]-network files.'
            )    

    parser.add_argument('path', help='The name of the path containing the '
                        'mandelbrot-[number]-network files.')

    parser.add_argument('--to-screen', dest='to_screen', action='store_true',
                        help='This flag will force the script to draw the graphs'
                            'on screen as opposed to saving them to the hard '
                            'disk.')

    parser.add_argument('--number', dest='filter_number', type=int,
                        help='The parameter for this flag will be used to filter '
                            'the fractals which are processed. If a value is specified '
                            'only the fractal with the number matching the value will '
                            'be processed.')


    args = parser.parse_args()

    #Call main.
    main(args.path, args.to_screen, args.filter_number)
 
