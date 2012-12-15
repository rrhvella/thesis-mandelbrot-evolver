import argparse
import matplotlib.pyplot as plt
import networkx as nx
import os
import sys
import re

FILE_REGEX = 'mandelbrot-(?P<number>[0-9]*)-network.txt'
FLOAT_REGEX = '\-{0,1}[0-9]*\.{0,1}[0-9]*'
COMPLEX_REGEX = r'\((?P<real>{0}), (?P<imaginary>{0})\)'.format(FLOAT_REGEX)
OUTPUT_NODE_REGEX = r'(H|O)\([0-9]*, (?P<function>.*)\)'

SEPERATOR = "( - | -> )" 

NODE_SIZE = 800
NUMBER_OF_ITERATIONS = 0

INPUT_LABELS = {'B(1)': '1', 'I(2)': 'z', 'I(3)': 'c'}
PRE_POSITIONS = {'B(1)': (-4, -4), 'I(2)': (0, -4), 'I(3)': (4, -4), 
                    'O(0, x)': (0, 4)}

FROM_INDEX = 0
TO_INDEX = 4
WEIGHT_INDEX = 2

def parse_complex(complex_string):
    m = re.match(COMPLEX_REGEX, complex_string)

    return complex(float(m.group('real')), float(m.group('imaginary')))


def parse_edge(edge_string):
    elements = [el.strip() for el in re.split(SEPERATOR, edge_string)]
    return (elements[FROM_INDEX], elements[TO_INDEX],
            parse_complex(elements[WEIGHT_INDEX]))

def output_node_label(node_description):
    return re.match(OUTPUT_NODE_REGEX, node_description).group('function')

def parse_filenames(path):
    for filename in os.listdir(path):
        match = re.match(FILE_REGEX, filename)

        if match is not None:
            yield filename, int(match.group('number'))

def main(path, to_screen = False):
    for filename, number in sorted(parse_filenames(path), 
                                    lambda x, y : cmp(x[1], y[1])):


        print("Drawing graph {0}".format(number))

        with open('{0}/{1}'.format(path, filename), 'r') as filebuf:
            graph = nx.DiGraph()
            
            edge_labels = {}
            node_labels = {}

            for line in filebuf:
                frm, to, weight = parse_edge(line)

                graph.add_edge(frm, to)

                edge_labels[(frm, to)] = weight
                node_labels[to] = output_node_label(to)

            for k, v in INPUT_LABELS.items():
                if k in graph.nodes():
                    node_labels[k] = v

            filtered_positions = {k: v for k, v in PRE_POSITIONS.items()
                                    if k in graph.nodes()}

            pos = nx.layout.spring_layout(graph, 
                                        pos = filtered_positions,
                                        fixed = filtered_positions.keys(),
                                        iterations = NUMBER_OF_ITERATIONS
                                        )

            nx.draw_networkx(graph, pos, node_size = NODE_SIZE, 
                            labels = node_labels)
            nx.draw_networkx_edge_labels(graph, pos, rotate=False, 
                                        edge_labels = edge_labels)

            if to_screen:
                plt.show()
            else:
                plt.savefig('{0}/mandelbrot-{1}-network-graph.png'
                                .format(path, number))
            plt.clf()
            


if __name__ == '__main__':
    parser = argparse.ArgumentParser(
            description='Generates graphs from network description '
                'in mandelbrot-[number]-network files.'
            )    

    parser.add_argument('path',
            help='The name of the path containing the '
            'mandelbrot-[number]-network files.')

    parser.add_argument('--to-screen', dest='to_screen', action='store_true',
            help='This flag will force the script to draw the graphs on screen '
                'as opposed to saving them to the hard disk.')

    args = parser.parse_args()
    main(args.path, args.to_screen)
 
